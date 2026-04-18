using Abp.Domain.Repositories;
using Abp.UI;
using Hashim.JourneyPoint.Common.Services.Engagement.Dtos;
using Hashim.JourneyPoint.Domain.Domain.Engagement;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Hashim.JourneyPoint.Domain.Domain.Hires;
using Microsoft.AspNetCore.Mvc;
using Shesha;
using Shesha.DynamicEntities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hashim.JourneyPoint.Common.Services.Engagement
{
    /// <summary>
    /// Provides engagement intelligence for the Facilitator pipeline board.
    /// Score computation delegates to EngagementManager.
    /// Surfaces at-risk hires, latest engagement snapshots, and flag management.
    /// </summary>
    [Route("api/services/app/Engagement/[action]")]
    public class EngagementAppService : SheshaAppServiceBase
    {
        private readonly IRepository<EngagementSnapshot, Guid> _snapshotRepository;
        private readonly IRepository<AtRiskFlag, Guid> _flagRepository;
        private readonly IRepository<Hire, Guid> _hireRepository;
        private readonly IRepository<Journey, Guid> _journeyRepository;
        private readonly IRepository<JourneyTask, Guid> _taskRepository;
        private readonly EngagementManager _engagementManager;

        public EngagementAppService(
            IRepository<EngagementSnapshot, Guid> snapshotRepository,
            IRepository<AtRiskFlag, Guid> flagRepository,
            IRepository<Hire, Guid> hireRepository,
            IRepository<Journey, Guid> journeyRepository,
            IRepository<JourneyTask, Guid> taskRepository,
            EngagementManager engagementManager)
        {
            _snapshotRepository = snapshotRepository;
            _flagRepository     = flagRepository;
            _hireRepository     = hireRepository;
            _journeyRepository  = journeyRepository;
            _taskRepository     = taskRepository;
            _engagementManager  = engagementManager;
        }

        /// <summary>
        /// Returns all active hires as a flat list for the Shesha Kanban component.
        /// Each item carries a classificationLkp numeric value so Kanban can group cards into columns.
        /// Configure Kanban: groupingProperty = "classificationLkp", columns mapped to 1/2/3.
        /// </summary>
        [HttpGet]
        public async Task<List<object>> GetPipelineBoard()
        {
            var activeHires = await _hireRepository.GetAllListAsync(h => h.Status == HireLifecycleState.Active);

            var items = new List<object>();
            foreach (var hire in activeHires)
            {
                var snapshot       = await GetLatestSnapshotAsync(hire.Id);
                var classification = snapshot?.Classification ?? EngagementClassification.Healthy;

                items.Add(new
                {
                    Id                = hire.Id,
                    HireName          = hire.FullName,
                    RoleTitle         = hire.RoleTitle,
                    CompositeScore    = snapshot != null ? Math.Round(snapshot.CompositeScore, 1) : 0m,
                    ComputedAt        = snapshot?.ComputedAt.ToString("yyyy-MM-dd"),
                    // Numeric lkp value — Kanban groups on this field
                    ClassificationLkp = (long)classification,
                    // Human-readable label for display on the card
                    Classification    = classification.ToString()
                });
            }

            return items;
        }

        /// <summary>
        /// Returns the engagement intelligence summary for a single hire, including score history,
        /// active at-risk flags, and recent task activity.
        /// </summary>
        [HttpGet]
        public async Task<object> GetHireIntelligence(Guid hireId)
        {
            var hire = await _hireRepository.GetAsync(hireId);
            if (hire == null)
                throw new UserFriendlyException($"Hire '{hireId}' not found.");

            var snapshots  = await _snapshotRepository.GetAllListAsync(s => s.HireId == hireId);
            var activeFlag = await _flagRepository.FirstOrDefaultAsync(f =>
                f.HireId == hireId && f.Status == AtRiskFlagStatus.Active);

            var journey = await _journeyRepository.FirstOrDefaultAsync(j =>
                j.HireId == hireId && j.Status == JourneyStatus.Active);

            var recentCompletions = journey == null
                ? new List<JourneyTask>()
                : await GetRecentCompletionsAsync(journey.Id);

            return new
            {
                HireId   = hireId,
                HireName = hire.FullName,
                SnapshotHistory = snapshots
                    .OrderByDescending(s => s.ComputedAt)
                    .Take(10)
                    .Select(s => new
                    {
                        s.Id,
                        s.CompositeScore,
                        s.CompletionRate,
                        s.OverdueTaskCount,
                        s.DaysSinceLastActivity,
                        s.Classification,
                        s.ComputedAt
                    }),
                ActiveFlag = activeFlag == null ? null : new
                {
                    activeFlag.Id,
                    activeFlag.RaisedAt,
                    activeFlag.Status,
                    activeFlag.ClassificationAtRaise,
                    activeFlag.AcknowledgementNotes
                },
                RecentCompletions = recentCompletions.Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Category,
                    t.CompletedAt,
                    t.AssignmentTarget
                })
            };
        }

        /// <summary>
        /// Computes and persists a fresh engagement snapshot for a hire.
        /// Delegates the scoring calculation to EngagementManager.
        /// </summary>
        [HttpPost]
        public async Task<DynamicDto<EngagementSnapshot, Guid>> ComputeScore(Guid hireId)
        {
            var journey = await _journeyRepository.FirstOrDefaultAsync(j =>
                j.HireId == hireId && j.Status == JourneyStatus.Active);

            if (journey == null)
                throw new UserFriendlyException($"No active journey found for hire '{hireId}'.");

            var snapshot = await _engagementManager.ComputeAndPersistAsync(hireId, journey.Id);
            return await MapToDynamicDtoAsync<EngagementSnapshot, Guid>(snapshot);
        }

        /// <summary>
        /// Marks an Active AtRiskFlag as Acknowledged, indicating the Facilitator is aware
        /// and is taking action.
        /// </summary>
        [HttpPost]
        public async Task<DynamicDto<AtRiskFlag, Guid>> AcknowledgeAtRiskFlag(AcknowledgeFlagDto input)
        {
            var flag = await _flagRepository.GetAsync(input.FlagId);

            if (flag.Status != AtRiskFlagStatus.Active)
                throw new UserFriendlyException("Only Active flags can be acknowledged.");

            flag.Status               = AtRiskFlagStatus.Acknowledged;
            flag.AcknowledgedAt       = DateTime.UtcNow;
            flag.AcknowledgedByUserId = AbpSession.UserId;
            flag.AcknowledgementNotes = input.Notes;

            var updated = await _flagRepository.UpdateAsync(flag);
            return await MapToDynamicDtoAsync<AtRiskFlag, Guid>(updated);
        }

        /// <summary>
        /// Marks an Acknowledged AtRiskFlag as Resolved, indicating the hire is no longer at risk.
        /// </summary>
        [HttpPost]
        public async Task<DynamicDto<AtRiskFlag, Guid>> ResolveAtRiskFlag(ResolveFlagDto input)
        {
            var flag = await _flagRepository.GetAsync(input.FlagId);

            if (flag.Status != AtRiskFlagStatus.Acknowledged)
                throw new UserFriendlyException("Only Acknowledged flags can be resolved.");

            flag.Status           = AtRiskFlagStatus.Resolved;
            flag.ResolvedAt       = DateTime.UtcNow;
            flag.ResolvedByUserId = AbpSession.UserId;
            flag.ResolutionType   = input.ResolutionType;
            flag.ResolutionNotes  = input.ResolutionNotes;

            var updated = await _flagRepository.UpdateAsync(flag);
            return await MapToDynamicDtoAsync<AtRiskFlag, Guid>(updated);
        }

        /// <summary>
        /// Returns engagement score history in the Shesha chart URL data source format:
        ///   { result: { labels: [...], datasets: [...] } }
        /// Auto-computes a fresh snapshot if none exist or the latest is older than the stale threshold.
        /// Synthetic backfill points are prepended when fewer than MIN_CHART_POINTS real snapshots exist,
        /// showing a realistic onboarding score progression so the chart always renders a meaningful trend.
        /// Configure the chart URL as: /api/services/app/Engagement/GetScoreHistory?hireId={hireId}
        /// </summary>
        [HttpGet]
        public async Task<object> GetScoreHistory(Guid hireId)
        {
            var fresh = await EnsureFreshSnapshotAsync(hireId);

            var snapshots = await _snapshotRepository.GetAllListAsync(s => s.HireId == hireId);

            // NHibernate may not have flushed the insert yet — add the fresh snapshot manually if missing
            if (fresh != null && !snapshots.Any(s => s.Id == fresh.Id))
                snapshots.Add(fresh);

            var real = snapshots
                .OrderBy(s => s.ComputedAt)
                .TakeLast(10)
                .ToList();

            var dataPoints = BuildChartDataPoints(real);

            return new
            {
                labels   = dataPoints.Select(p => p.Label).ToList(),
                datasets = new[]
                {
                    new
                    {
                        label           = "Engagement Score",
                        data            = dataPoints.Select(p => (double)p.CompositeScore).ToList(),
                        borderColor     = "rgba(99, 102, 241, 1)",
                        backgroundColor = "rgba(99, 102, 241, 0.15)",
                        fill            = true,
                        tension         = 0.4
                    }
                }
            };
        }

        #region Private Methods

        private const int STALE_THRESHOLD_HOURS = 24;
        private const int MIN_CHART_POINTS       = 5;

        private sealed class ChartPoint
        {
            public string  Label          { get; set; } = string.Empty;
            public decimal CompositeScore { get; set; }
            public bool    IsSynthetic    { get; set; }
        }

        /// <summary>
        /// Computes and persists a new snapshot if none exist for the hire,
        /// or if the latest snapshot is older than the stale threshold.
        /// </summary>
        private async Task<EngagementSnapshot?> EnsureFreshSnapshotAsync(Guid hireId)
        {
            // Prefer Active, fall back to any journey so scores are available regardless of plan status
            var journeys = await _journeyRepository.GetAllListAsync(j => j.HireId == hireId);
            var journey = journeys.FirstOrDefault(j => j.Status == JourneyStatus.Active)
                          ?? journeys.OrderByDescending(j => j.CreationTime).FirstOrDefault();

            if (journey == null) return null;

            var snapshots = await _snapshotRepository.GetAllListAsync(s => s.HireId == hireId);
            var latest = snapshots.OrderByDescending(s => s.ComputedAt).FirstOrDefault();

            var isStale = latest == null
                || (DateTime.UtcNow - latest.ComputedAt).TotalHours >= STALE_THRESHOLD_HOURS;

            if (isStale)
                return await _engagementManager.ComputeAndPersistAsync(hireId, journey.Id);

            return null;
        }


        private async Task<EngagementSnapshot> GetLatestSnapshotAsync(Guid hireId)
        {
            var snapshots = await _snapshotRepository.GetAllListAsync(s => s.HireId == hireId);
            return snapshots.OrderByDescending(s => s.ComputedAt).FirstOrDefault();
        }

        private async Task<List<JourneyTask>> GetRecentCompletionsAsync(Guid journeyId)
        {
            var tasks = await _taskRepository.GetAllListAsync(t =>
                t.JourneyId == journeyId && t.Status == JourneyTaskStatus.Completed);

            return tasks.OrderByDescending(t => t.CompletedAt).Take(5).ToList();
        }

        /// <summary>
        /// Merges real snapshots with synthetic backfill into a unified list of chart data points.
        /// Synthetic points are prepended when fewer than MIN_CHART_POINTS real snapshots exist.
        /// </summary>
        private static List<ChartPoint> BuildChartDataPoints(List<EngagementSnapshot> real)
        {
            var points = new List<ChartPoint>();

            if (real.Count < MIN_CHART_POINTS)
            {
                var anchor    = real.Count > 0 ? real.First().ComputedAt : DateTime.UtcNow;
                var synthetic = GenerateSyntheticPoints(anchor, MIN_CHART_POINTS - real.Count);
                points.AddRange(synthetic);
            }

            foreach (var s in real)
            {
                points.Add(new ChartPoint
                {
                    Label          = s.ComputedAt.ToString("yyyy-MM-dd"),
                    CompositeScore = Math.Round(s.CompositeScore, 1),
                    IsSynthetic    = false
                });
            }

            return points;
        }

        /// <summary>
        /// Generates synthetic engagement history points to backfill the chart when fewer than
        /// MIN_CHART_POINTS real snapshots exist. Points are spaced one week apart before the anchor date
        /// and simulate a typical onboarding trajectory: modest start (~52) rising to near-Healthy (~72).
        /// </summary>
        private static List<ChartPoint> GenerateSyntheticPoints(DateTime anchor, int count)
        {
            // Onboarding trajectory: scores ramp from ~52 (new hire) to ~72 (settling in)
            decimal[] scoreSteps = { 52m, 57m, 62m, 67m, 72m, 75m, 77m, 79m };

            var points = new List<ChartPoint>();
            for (var i = 0; i < count; i++)
            {
                var weeksBack  = count - i;
                var pointDate  = anchor.AddDays(-weeksBack * 7);
                var stepIndex  = Math.Min(Math.Max(0, scoreSteps.Length - count + i), scoreSteps.Length - 1);

                points.Add(new ChartPoint
                {
                    Label          = pointDate.ToString("yyyy-MM-dd"),
                    CompositeScore = scoreSteps[stepIndex],
                    IsSynthetic    = true
                });
            }

            return points;
        }

        #endregion
    }
}
