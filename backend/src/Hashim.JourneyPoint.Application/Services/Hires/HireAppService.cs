using Abp.Domain.Repositories;
using Abp.UI;
using Hashim.JourneyPoint.Common.Services.AI;
using Hashim.JourneyPoint.Common.Services.Hires.Dtos;
using Hashim.JourneyPoint.Domain;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Hashim.JourneyPoint.Domain.Domain.Hires;
using Hashim.JourneyPoint.Domain.Domain.OnboardingPlans;
using Hashim.JourneyPoint.Domain.Domain.Wellness;
using Microsoft.AspNetCore.Mvc;
using Shesha;
using Shesha.Authorization.Users;
using Shesha.DynamicEntities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hashim.JourneyPoint.Common.Services.Hires
{
    /// <summary>
    /// Manages Hire records. The Facilitator uses this service to create new hires
    /// and view hire details. Creating a hire automatically:
    ///  - Provisions an ABP user account with the Enrolee role.
    ///  - Generates and activates the onboarding Journey.
    ///  - Schedules all 9 wellness check-ins.
    ///  - Seeds AI-authored check-in questions via Groq.
    /// No separate activation step is required.
    /// </summary>
    [Route("api/services/app/Hire/[action]")]
    public class HireAppService : SheshaAppServiceBase
    {
        #region Dependencies

        private readonly IRepository<Hire, Guid>          _hireRepository;
        private readonly IRepository<OnboardingPlan, Guid> _planRepository;
        private readonly HireJourneyManager               _journeyManager;
        private readonly WellnessManager                  _wellnessManager;
        private readonly GroqWellnessService              _groqWellness;
        private readonly UserManager                      _userManager;

        #endregion

        public HireAppService(
            IRepository<Hire, Guid>          hireRepository,
            IRepository<OnboardingPlan, Guid> planRepository,
            HireJourneyManager               journeyManager,
            WellnessManager                  wellnessManager,
            GroqWellnessService              groqWellness,
            UserManager                      userManager)
        {
            _hireRepository  = hireRepository;
            _planRepository  = planRepository;
            _journeyManager  = journeyManager;
            _wellnessManager = wellnessManager;
            _groqWellness    = groqWellness;
            _userManager     = userManager;
        }

        /// <summary>Returns all hires.</summary>
        [HttpGet]
        public async Task<List<DynamicDto<Hire, Guid>>> GetHires()
        {
            var hires  = await _hireRepository.GetAllListAsync();
            var result = new List<DynamicDto<Hire, Guid>>();
            foreach (var hire in hires)
                result.Add(await MapToDynamicDtoAsync<Hire, Guid>(hire));
            return result;
        }

        /// <summary>Returns full details of a single hire.</summary>
        [HttpGet]
        public async Task<DynamicDto<Hire, Guid>> GetDetail(Guid id)
        {
            var hire = await _hireRepository.GetAsync(id);
            return await MapToDynamicDtoAsync<Hire, Guid>(hire);
        }

        /// <summary>
        /// Creates a hire and, in one atomic sequence:
        /// 1. Provisions an ABP user account for the hire and assigns the Enrolee role.
        /// 2. Copies the OnboardingPlan's tasks into a Journey.
        /// 3. Activates the Journey (hire moves to Active immediately).
        /// 4. Schedules all 9 wellness check-ins.
        /// 5. Generates AI-authored check-in questions via Groq.
        /// </summary>
        [HttpPost]
        public async Task<DynamicDto<Hire, Guid>> Create(CreateHireDto input)
        {
            var planId = input.OnboardingPlan?.Id ?? Guid.Empty;
            var plan = await _planRepository.GetAsync(planId);
            if (plan == null)
                throw new UserFriendlyException($"Onboarding plan '{planId}' not found.");

            // Provision ABP user account with Enrolee role
            var platformUser = await ProvisionEnroleeAccountAsync(input.FullName, input.EmailAddress);

            var hire = await _hireRepository.InsertAsync(new Hire
            {
                FullName                  = input.FullName,
                EmailAddress              = input.EmailAddress,
                RoleTitle                 = input.RoleTitle,
                Department                = input.Department,
                StartDate                 = input.StartDate,
                OnboardingPlanId          = planId,
                OnboardingPlan            = plan,
                ManagerUserId             = input.ManagerUserId,
                PlatformUserId            = platformUser.Id,
                Status                    = HireLifecycleState.PendingActivation,
                WelcomeNotificationStatus = WelcomeNotificationStatus.Pending
            });

            // Generate journey (copies plan tasks) then activate immediately
            var journey = await _journeyManager.CreateDraftJourneyAsync(hire.Id);
            var active  = await _journeyManager.ActivateJourneyAsync(journey.Id);

            // Schedule wellness check-ins across 9 milestone periods
            await _wellnessManager.GenerateCheckInsForJourneyAsync(hire.Id, active.Id);

            // Generate AI questions for each check-in (non-blocking — errors are logged, not thrown)
            await _groqWellness.GenerateQuestionsForJourneyAsync(hire.Id, active.Id);

            // Reload to return the hire with its updated Active status
            var updated = await _hireRepository.GetAsync(hire.Id);
            return await MapToDynamicDtoAsync<Hire, Guid>(updated);
        }

        /// <summary>Resends the welcome onboarding notification email.</summary>
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

        #region Private Methods

        /// <summary>
        /// Creates an ABP user account for the hire and assigns the Enrolee role.
        /// Username = email address. A temporary password is generated; the hire
        /// should reset it on first login via the forgot-password flow.
        /// </summary>
        private async Task<User> ProvisionEnroleeAccountAsync(string fullName, string email)
        {
            var nameParts = fullName.Trim().Split(' ', 2);
            var firstName = nameParts[0];
            var lastName  = nameParts.Length > 1 ? nameParts[1] : ".";
            var tempPass  = GenerateTempPassword();

            var user = await _userManager.CreateUserAsync(
                username:                    email,
                createLocalPassword:         true,
                password:                    tempPass,
                passwordConfirmation:        tempPass,
                firstname:                   firstName,
                lastname:                    lastName,
                mobileNumber:                string.Empty,
                emailAddress:                email,
                supportedPasswordResetMethods: null);

            await _userManager.AddToRoleAsync(user, JourneyPointRoles.Enrolee);
            return user;
        }

        private static string GenerateTempPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var rng    = new Random();
            var suffix = new string(Enumerable.Range(0, 8).Select(_ => chars[rng.Next(chars.Length)]).ToArray());
            return $"Hire@{suffix}";
        }

        #endregion
    }
}
