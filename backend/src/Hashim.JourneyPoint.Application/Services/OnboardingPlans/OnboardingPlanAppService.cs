using Abp.Domain.Repositories;
using Abp.UI;
using Hashim.JourneyPoint.Common.Services.OnboardingPlans.Dtos;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Hashim.JourneyPoint.Domain.Domain.OnboardingPlans;
using Microsoft.AspNetCore.Mvc;
using Shesha;
using Shesha.DynamicEntities.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hashim.JourneyPoint.Common.Services.OnboardingPlans
{
    /// <summary>
    /// Manages OnboardingPlan templates. Facilitators create, publish, and maintain plans.
    /// Published plans cannot be modified — clone and edit the clone instead.
    /// Deep-clone operations delegate to OnboardingPlanManager.
    /// </summary>
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

        /// <summary>Returns a list of all OnboardingPlans for the current tenant.</summary>
        [HttpGet, Route("[action]")]
        public async Task<List<DynamicDto<OnboardingPlan, Guid>>> GetPlans()
        {
            var plans = await _planRepository.GetAllListAsync();
            var result = new List<DynamicDto<OnboardingPlan, Guid>>();
            foreach (var plan in plans)
                result.Add(await MapToDynamicDtoAsync<OnboardingPlan, Guid>(plan));
            return result;
        }

        /// <summary>Returns full detail of a single OnboardingPlan including its modules and tasks.</summary>
        [HttpGet, Route("[action]")]
        public async Task<DynamicDto<OnboardingPlan, Guid>> GetDetail(Guid id)
        {
            var plan = await _planRepository.GetAsync(id);
            return await MapToDynamicDtoAsync<OnboardingPlan, Guid>(plan);
        }

        /// <summary>Creates a new OnboardingPlan in Draft status.</summary>
        [HttpPost, Route("[action]")]
        public async Task<DynamicDto<OnboardingPlan, Guid>> Create(CreateOnboardingPlanDto input)
        {
            var plan = new OnboardingPlan
            {
                Name           = input.Name,
                Description    = input.Description,
                TargetAudience = input.TargetAudience,
                DurationDays   = input.DurationDays,
                Status         = OnboardingPlanStatus.Draft
            };

            var saved = await _planRepository.InsertAsync(plan);
            return await MapToDynamicDtoAsync<OnboardingPlan, Guid>(saved);
        }

        /// <summary>Updates a Draft OnboardingPlan. Published plans cannot be updated directly — clone first.</summary>
        [HttpPut, Route("[action]")]
        public async Task<DynamicDto<OnboardingPlan, Guid>> Update(UpdateOnboardingPlanDto input)
        {
            var plan = await _planRepository.GetAsync(input.Id);

            if (plan.Status == OnboardingPlanStatus.Published)
                throw new UserFriendlyException("Published plans cannot be edited. Clone the plan and edit the copy.");

            if (input.Name != null) plan.Name = input.Name;
            if (input.Description != null) plan.Description = input.Description;
            if (input.TargetAudience != null) plan.TargetAudience = input.TargetAudience;
            if (input.DurationDays.HasValue) plan.DurationDays = input.DurationDays.Value;

            var updated = await _planRepository.UpdateAsync(plan);
            return await MapToDynamicDtoAsync<OnboardingPlan, Guid>(updated);
        }

        /// <summary>Publishes a Draft OnboardingPlan, making it available for hire enrolment.</summary>
        [HttpPost, Route("[action]")]
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
        [HttpPost, Route("[action]")]
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
        [HttpPost, Route("[action]")]
        public async Task<DynamicDto<OnboardingPlan, Guid>> Clone(Guid id)
        {
            var clone = await _planManager.ClonePlanAsync(id);
            return await MapToDynamicDtoAsync<OnboardingPlan, Guid>(clone);
        }

        /// <summary>Sends the plan's task list to Groq and stores AI enhancement suggestions as a GenerationLog.</summary>
        [HttpPost, Route("[action]")]
        public async Task<DynamicDto<OnboardingPlan, Guid>> EnhancePlanWithAi(Guid id)
        {
            // TODO: delegate to GroqExtractionService, create GenerationLog record
            throw new NotImplementedException("EnhancePlanWithAi: call GroqExtractionService, create GenerationLog");
        }

        /// <summary>Applies the previously generated AI enhancement suggestions to the plan's tasks.</summary>
        [HttpPost, Route("[action]")]
        public async Task<DynamicDto<OnboardingPlan, Guid>> ApplyPlanEnhancement(Guid id)
        {
            // TODO: load the latest GenerationLog for this plan and apply suggestions to OnboardingTask records
            throw new NotImplementedException("ApplyPlanEnhancement: load GenerationLog and update OnboardingTasks");
        }
    }
}
