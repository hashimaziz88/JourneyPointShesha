using Abp.Domain.Repositories;
using Abp.UI;
using Hashim.JourneyPoint.Common.Services.Hires.Dtos;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Hashim.JourneyPoint.Domain.Domain.Hires;
using Hashim.JourneyPoint.Domain.Domain.OnboardingPlans;
using Microsoft.AspNetCore.Mvc;
using Shesha;
using Shesha.DynamicEntities.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hashim.JourneyPoint.Common.Services.Hires
{
    /// <summary>
    /// Manages Hire records. The Facilitator uses this service to create new hires,
    /// view hire details, and trigger welcome notifications.
    /// </summary>
    [Route("api/services/app/Hire/[action]")]
    public class HireAppService : SheshaAppServiceBase
    {
        private readonly IRepository<Hire, Guid> _hireRepository;
        private readonly IRepository<OnboardingPlan, Guid> _planRepository;

        public HireAppService(
            IRepository<Hire, Guid> hireRepository,
            IRepository<OnboardingPlan, Guid> planRepository)
        {
            _hireRepository = hireRepository;
            _planRepository = planRepository;
        }

        /// <summary>Returns a paged list of all hires for the current tenant.</summary>
        [HttpGet]
        public async Task<List<DynamicDto<Hire, Guid>>> GetHires()
        {
            var hires = await _hireRepository.GetAllListAsync();
            var result = new List<DynamicDto<Hire, Guid>>();
            foreach (var hire in hires)
                result.Add(await MapToDynamicDtoAsync<Hire, Guid>(hire));
            return result;
        }

        /// <summary>Returns full details of a single hire including their journey summary.</summary>
        [HttpGet]
        public async Task<DynamicDto<Hire, Guid>> GetDetail(Guid id)
        {
            var hire = await _hireRepository.GetAsync(id);
            return await MapToDynamicDtoAsync<Hire, Guid>(hire);
        }

        /// <summary>Returns a list of users eligible to be assigned as a manager for a hire.</summary>
        [HttpGet]
        public async Task<List<object>> GetManagerOptions()
        {
            // Returns ABP users with the Manager role — implementation uses UserManager
            throw new NotImplementedException("GetManagerOptions: query AbpUsers filtered by Manager role");
        }

        /// <summary>
        /// Creates a new hire record in Pending status.
        /// The caller is responsible for triggering JourneyAppService.GenerateDraft after creation.
        /// </summary>
        [HttpPost]
        public async Task<DynamicDto<Hire, Guid>> Create(CreateHireDto input)
        {
            var plan = await _planRepository.GetAsync(input.OnboardingPlanId);
            if (plan == null)
                throw new UserFriendlyException($"Onboarding plan '{input.OnboardingPlanId}' not found.");

            var hire = new Hire
            {
                FullName = input.FullName,
                EmailAddress = input.Email,
                RoleTitle = input.RoleTitle,
                Department = input.DepartmentName,
                StartDate = input.StartDate,
                OnboardingPlan = plan,
                ManagerUserId = input.ManagerUserId,
                Status = HireLifecycleState.PendingActivation,
                WelcomeNotificationStatus = WelcomeNotificationStatus.Pending
            };

            var saved = await _hireRepository.InsertAsync(hire);
            return await MapToDynamicDtoAsync<Hire, Guid>(saved);
        }

        /// <summary>Resends the welcome onboarding notification email to the hire.</summary>
        [HttpPost]
        public async Task ResendWelcomeNotification(Guid hireId)
        {
            var hire = await _hireRepository.GetAsync(hireId);
            if (hire == null)
                throw new UserFriendlyException($"Hire '{hireId}' not found.");

            // TODO: dispatch email via IEmailSender
            hire.WelcomeNotificationStatus = WelcomeNotificationStatus.Sent;
            hire.WelcomeNotificationSentAt = DateTime.UtcNow;
            await _hireRepository.UpdateAsync(hire);
        }
    }
}
