using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Hashim.JourneyPoint.Domain.Domain.Hires;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hashim.JourneyPoint.Domain.Domain.Engagement
{
    /// <summary>
    /// Computes engagement scores for hire journeys and manages the at-risk flag lifecycle.
    /// Snapshots are append-only — each call inserts a new record, never updates an existing one.
    /// At most one Active flag per hire is maintained at any time.
    /// </summary>
    public class EngagementManager : DomainService
    {
        #region Constants

        private const decimal COMPLETION_WEIGHT          = 0.5m;
        private const decimal RECENCY_WEIGHT             = 0.3m;
        private const decimal OVERDUE_WEIGHT             = 0.2m;
        private const decimal HEALTHY_THRESHOLD          = 70m;
        private const decimal NEEDS_ATTENTION_THRESHOLD  = 40m;
        private const int     RECENCY_WINDOW_DAYS        = 7;
        private const decimal OVERDUE_PENALTY_PER_TASK   = 0.15m;
        private const decimal MAX_SCORE                  = 100m;
        private const decimal MIN_SCORE                  = 0m;
        /// <summary>Stored when the hire has not yet completed any tasks. -1 distinguishes "no activity" from "active today".</summary>
        private const int     NO_ACTIVITY_SENTINEL       = -1;

        #endregion

        #region Dependencies

        private readonly IRepository<EngagementSnapshot, Guid> _snapshotRepository;
        private readonly IRepository<AtRiskFlag, Guid> _flagRepository;
        private readonly IRepository<JourneyTask, Guid> _taskRepository;

        #endregion

        public EngagementManager(
            IRepository<EngagementSnapshot, Guid> snapshotRepository,
            IRepository<AtRiskFlag, Guid> flagRepository,
            IRepository<JourneyTask, Guid> taskRepository)
        {
            _snapshotRepository = snapshotRepository;
            _flagRepository = flagRepository;
            _taskRepository = taskRepository;
        }

        #region Public Methods

        /// <summary>
        /// Computes a new engagement snapshot for the given journey and persists it.
        /// May raise or auto-resolve an AtRiskFlag depending on the resulting classification.
        /// Always inserts a new snapshot — never updates an existing one.
        /// </summary>
        public async Task<EngagementSnapshot> ComputeAndPersistAsync(Guid hireId, Guid journeyId)
        {
            var today = DateTime.UtcNow.Date;
            var tasks = await _taskRepository.GetAllListAsync(t => t.JourneyId == journeyId);

            var snapshot = BuildSnapshot(hireId, journeyId, tasks, today);
            await _snapshotRepository.InsertAsync(snapshot);
            await HandleAtRiskFlagAsync(hireId, journeyId, snapshot.Classification);
            return snapshot;
        }

        #endregion

        #region Private Scoring Methods

        private EngagementSnapshot BuildSnapshot(Guid hireId, Guid journeyId, IList<JourneyTask> tasks, DateTime today)
        {
            var completionRate  = ComputeCompletionRate(tasks, today);
            var recencyScore    = ComputeRecencyScore(tasks, today);
            var overduePenalty  = ComputeOverduePenalty(tasks, today);
            var compositeScore  = ComputeCompositeScore(completionRate, recencyScore, overduePenalty);
            var classification  = Classify(compositeScore);

            return new EngagementSnapshot
            {
                HireId                = hireId,
                JourneyId             = journeyId,
                CompletionRate        = Math.Floor(completionRate * 10000m) / 10000m,
                DaysSinceLastActivity = GetDaysSinceLastActivity(tasks, today),
                OverdueTaskCount      = tasks.Count(t => t.DueOn.Date < today && t.Status == JourneyTaskStatus.Pending),
                CompositeScore        = compositeScore,
                Classification        = classification,
                ComputedAt            = DateTime.UtcNow
            };
        }

        private decimal ComputeCompletionRate(IList<JourneyTask> tasks, DateTime today)
        {
            var dueTasks       = tasks.Count(t => t.DueOn.Date <= today);
            var completedTasks = tasks.Count(t => t.Status == JourneyTaskStatus.Completed);

            if (dueTasks == 0) return 1m;
            return Math.Min(1m, completedTasks / (decimal)dueTasks);
        }

        private decimal ComputeRecencyScore(IList<JourneyTask> tasks, DateTime today)
        {
            var lastCompletion = tasks
                .Where(t => t.CompletedAt.HasValue)
                .Select(t => t.CompletedAt!.Value.Date)
                .OrderByDescending(d => d)
                .FirstOrDefault();

            // No tasks are due yet — hire is in the onboarding grace period, treat as fully active
            if (lastCompletion == default && !tasks.Any(t => t.DueOn.Date <= today)) return 1m;

            if (lastCompletion == default) return 0m;

            var daysSince = (today - lastCompletion).Days;
            return Math.Max(0m, 1m - daysSince / (decimal)RECENCY_WINDOW_DAYS);
        }

        private decimal ComputeOverduePenalty(IList<JourneyTask> tasks, DateTime today)
        {
            var overdueCount = tasks.Count(t => t.DueOn.Date < today && t.Status == JourneyTaskStatus.Pending);
            return Math.Min(1m, overdueCount * OVERDUE_PENALTY_PER_TASK);
        }

        private decimal ComputeCompositeScore(decimal completionRate, decimal recencyScore, decimal overduePenalty)
        {
            var raw = (completionRate * COMPLETION_WEIGHT)
                    + (recencyScore   * RECENCY_WEIGHT)
                    - (overduePenalty * OVERDUE_WEIGHT);

            return Math.Max(MIN_SCORE, Math.Min(MAX_SCORE, raw * MAX_SCORE));
        }

        private EngagementClassification Classify(decimal score) => score switch
        {
            >= HEALTHY_THRESHOLD         => EngagementClassification.Healthy,
            >= NEEDS_ATTENTION_THRESHOLD => EngagementClassification.NeedsAttention,
            _                            => EngagementClassification.AtRisk
        };

        private int GetDaysSinceLastActivity(IList<JourneyTask> tasks, DateTime today)
        {
            var lastDate = tasks
                .Where(t => t.CompletedAt.HasValue)
                .Select(t => t.CompletedAt!.Value.Date)
                .OrderByDescending(d => d)
                .FirstOrDefault();

            return lastDate == default ? NO_ACTIVITY_SENTINEL : (today - lastDate).Days;
        }

        #endregion

        #region Flag Management

        private async Task HandleAtRiskFlagAsync(Guid hireId, Guid journeyId, EngagementClassification classification)
        {
            var activeFlag = await _flagRepository.FirstOrDefaultAsync(
                f => f.HireId == hireId && f.Status == AtRiskFlagStatus.Active);

            if (classification == EngagementClassification.AtRisk && activeFlag == null)
            {
                await RaiseFlagAsync(hireId, journeyId, classification);
                return;
            }

            if (classification == EngagementClassification.Healthy && activeFlag != null)
                await AutoResolveFlagAsync(activeFlag);
        }

        private async Task RaiseFlagAsync(Guid hireId, Guid journeyId, EngagementClassification classification)
        {
            var flag = new AtRiskFlag
            {
                HireId               = hireId,
                JourneyId            = journeyId,
                RaisedAt             = DateTime.UtcNow,
                ClassificationAtRaise = classification,
                Status               = AtRiskFlagStatus.Active
            };
            await _flagRepository.InsertAsync(flag);
        }

        private async Task AutoResolveFlagAsync(AtRiskFlag flag)
        {
            flag.Status         = AtRiskFlagStatus.Resolved;
            flag.ResolvedAt     = DateTime.UtcNow;
            flag.ResolutionType = AtRiskResolutionType.AutomaticHealthyRecovery;
            flag.ResolutionNotes = "Auto-resolved: hire engagement recovered to Healthy.";
            await _flagRepository.UpdateAsync(flag);
        }

        #endregion
    }
}
