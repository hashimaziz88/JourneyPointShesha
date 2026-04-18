using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Hashim.JourneyPoint.Domain.Domain.OnboardingPlans;
using Microsoft.AspNetCore.Mvc;
using Shesha;
using Shesha.DynamicEntities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hashim.JourneyPoint.Common.Services.OnboardingPlans
{
    /// <summary>
    /// Custom endpoints for OnboardingModule.
    /// CRUD is handled by the Shesha dynamic API:
    ///   /api/dynamic/Hashim.JourneyPoint/OnboardingModule/Crud/
    /// This service exposes filtered list endpoints for use in form Data Contexts.
    /// </summary>
    [Route("api/services/app/OnboardingModule/[action]")]
    public class OnboardingModuleAppService : SheshaAppServiceBase
    {
        private readonly IRepository<OnboardingModule, Guid> _moduleRepository;

        public OnboardingModuleAppService(IRepository<OnboardingModule, Guid> moduleRepository)
        {
            _moduleRepository = moduleRepository;
        }

        /// <summary>
        /// Returns all modules belonging to the given plan, ordered by OrderIndex.
        /// Used as the Custom Endpoint on the modules Data Context in the plans-details form.
        /// URL: /api/services/app/OnboardingModule/GetByPlan?planId={id}
        /// Accepts planId as string to tolerate Shesha appending extra query params (e.g. ?name=...).
        /// </summary>
        [HttpGet]
        public async Task<PagedResultDto<DynamicDto<OnboardingModule, Guid>>> GetByPlan(string planId)
        {
            var clean = planId?.Split('?')[0]?.Trim();
            if (!Guid.TryParse(clean, out var guid))
                throw new Abp.UI.UserFriendlyException($"Invalid planId: {planId}");

            var modules = await _moduleRepository.GetAllListAsync(m => m.OnboardingPlanId == guid);
            var ordered = modules.OrderBy(m => m.OrderIndex).ToList();

            var dtos = new List<DynamicDto<OnboardingModule, Guid>>();
            foreach (var module in ordered)
                dtos.Add(await MapToDynamicDtoAsync<OnboardingModule, Guid>(module));

            return new PagedResultDto<DynamicDto<OnboardingModule, Guid>>(dtos.Count, dtos);
        }
    }
}
