using Abp.Domain.Repositories;
using Abp.UI;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Hashim.JourneyPoint.Domain.Domain.OnboardingPlans;
using Microsoft.AspNetCore.Mvc;
using Shesha;
using Shesha.DynamicEntities.Dtos;
using System;
using System.Threading.Tasks;

namespace Hashim.JourneyPoint.Common.Services.OnboardingPlans
{
    /// <summary>
    /// Custom lifecycle actions for OnboardingPlan templates.
    /// CRUD (Get, GetAll, Create, Update, Delete) is handled by the Shesha dynamic API:
    ///   /api/dynamic/Hashim.JourneyPoint/OnboardingPlan/Crud/
    /// This service exposes only state-transition and AI-enhancement endpoints.
    /// </summary>
    [Route("api/services/app/OnboardingPlan/[action]")]
    public class OnboardingPlanAppService : SheshaAppServiceBase
    {
        private readonly IRepository<OnboardingPlan, Guid> _planRepository;
        private readonly OnboardingPlanManager _planManager;

        public OnboardingPlanAppService(
            IRepository<OnboardingPlan, Guid> planRepository,
            OnboardingPlanManager planManager)
        {
            _planRepository = planRepository;
            _planManager    = planManager;
        }

        /// <summary>Publishes a Draft OnboardingPlan, making it available for hire enrolment.</summary>
        [HttpPost]
        public async Task<DynamicDto<OnboardingPlan, Guid>> Publish(Guid id)
        {
            var plan = await _planRepository.GetAsync(id);

            if (plan.Status != OnboardingPlanStatus.Draft)
                throw new UserFriendlyException("Only Draft plans can be published.");

            plan.Status = OnboardingPlanStatus.Published;
            var updated = await _planRepository.UpdateAsync(plan);
            return await MapToDynamicDtoAsync<OnboardingPlan, Guid>(updated);
        }

        /// <summary>Archives a Published plan so it can no longer be assigned to new hires.</summary>
        [HttpPost]
        public async Task<DynamicDto<OnboardingPlan, Guid>> Archive(Guid id)
        {
            var plan = await _planRepository.GetAsync(id);

            if (plan.Status != OnboardingPlanStatus.Published)
                throw new UserFriendlyException("Only Published plans can be archived.");

            plan.Status = OnboardingPlanStatus.Archived;
            var updated = await _planRepository.UpdateAsync(plan);
            return await MapToDynamicDtoAsync<OnboardingPlan, Guid>(updated);
        }

        /// <summary>Creates a Draft copy of a plan, preserving all modules and tasks.</summary>
        [HttpPost]
        public async Task<DynamicDto<OnboardingPlan, Guid>> Clone(Guid id)
        {
            var clone = await _planManager.ClonePlanAsync(id);
            return await MapToDynamicDtoAsync<OnboardingPlan, Guid>(clone);
        }

        /// <summary>Sends the plan's task list to Groq and stores AI enhancement suggestions as a GenerationLog.</summary>
        [HttpPost]
        public async Task<DynamicDto<OnboardingPlan, Guid>> EnhancePlanWithAi(Guid id)
        {
            // TODO: delegate to GroqExtractionService, create GenerationLog record
            throw new NotImplementedException("EnhancePlanWithAi: call GroqExtractionService, create GenerationLog");
        }

        /// <summary>Applies the previously generated AI enhancement suggestions to the plan's tasks.</summary>
        [HttpPost]
        public async Task<DynamicDto<OnboardingPlan, Guid>> ApplyPlanEnhancement(Guid id)
        {
            // TODO: load the latest GenerationLog for this plan and apply suggestions to OnboardingTask records
            throw new NotImplementedException("ApplyPlanEnhancement: load GenerationLog and update OnboardingTasks");
        }
    }
}
