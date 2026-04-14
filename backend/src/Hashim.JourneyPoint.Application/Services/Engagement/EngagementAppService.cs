using Abp.Application.Services.Dto;
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
        /// Returns all active hires grouped by engagement classification for the Kanban pipeline board.
        /// Each column (Healthy / NeedsAttention / AtRisk) contains hire summaries with latest scores.
        /// </summary>
        [HttpGet]
        public async Task<object> GetPipelineBoard()
        {
            var activeHires = await _hireRepository.GetAllListAsync(h => h.Status == HireLifecycleState.Active);
            var columns = new Dictionary<string, List<object>>
            {
                { EngagementClassification.Healthy.ToString(),         new List<object>() },
                { EngagementClassification.NeedsAttention.ToString(),  new List<object>() },
                { EngagementClassification.AtRisk.ToString(),          new List<object>() }
            };

            foreach (var hire in activeHires)
            {
                var summary = await BuildHireSummaryAsync(hire);
                columns[summary.Classification].Add(summary);
            }

            return columns;
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
                HireId            = hireId,
                HireName          = hire.FullName,
                SnapshotHistory   = snapshots.OrderByDescending(s => s.ComputedAt).Take(10),
                ActiveFlag        = activeFlag,
                RecentCompletions = recentCompletions
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
        /// Returns the last 10 engagement snapshots for a hire ordered chronologically.
        /// Used as the URL data source for the engagement score line chart.
        /// </summary>
        [HttpGet]
        public async Task<PagedResultDto<DynamicDto<EngagementSnapshot, Guid>>> GetScoreHistory(Guid hireId)
        {
            var snapshots = await _snapshotRepository.GetAllListAsync(s => s.HireId == hireId);
            var page = snapshots
                .OrderBy(s => s.ComputedAt)
                .TakeLast(10)
                .ToList();

            var items = new List<DynamicDto<EngagementSnapshot, Guid>>();
            foreach (var snapshot in page)
                items.Add(await MapToDynamicDtoAsync<EngagementSnapshot, Guid>(snapshot));

            return new PagedResultDto<DynamicDto<EngagementSnapshot, Guid>>(snapshots.Count, items);
        }

        #region Private Methods

        private async Task<dynamic> BuildHireSummaryAsync(Hire hire)
        {
            var latestSnapshot = await GetLatestSnapshotAsync(hire.Id);
            var classification = latestSnapshot?.Classification.ToString()
                                 ?? EngagementClassification.Healthy.ToString();

            return new
            {
                HireId         = hire.Id,
                HireName       = hire.FullName,
                RoleTitle      = hire.RoleTitle,
                Classification = classification,
                CompositeScore = latestSnapshot?.CompositeScore ?? 0m,
                ComputedAt     = latestSnapshot?.ComputedAt
            };
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

        #endregion
    }
}
