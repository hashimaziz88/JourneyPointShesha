using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Ardalis.GuardClauses;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Hashim.JourneyPoint.Domain.Domain.Hires;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hashim.JourneyPoint.Domain.Domain.Wellness
{
    /// <summary>
    /// Manages the creation and scheduling of WellnessCheckIn records for a hire's journey.
    /// Check-ins are generated at journey activation across 9 milestone periods
    /// (Day1 through Month6). Submission validation enforces that all questions are answered.
    /// </summary>
    public class WellnessManager : DomainService
    {
        #region Constants

        private const int DAY1_OFFSET   = 1;
        private const int DAY2_OFFSET   = 2;
        private const int WEEK1_OFFSET  = 7;
        private const int MONTH1_OFFSET = 30;
        private const int MONTH2_OFFSET = 60;
        private const int MONTH3_OFFSET = 90;
        private const int MONTH4_OFFSET = 120;
        private const int MONTH5_OFFSET = 150;
        private const int MONTH6_OFFSET = 180;

        #endregion

        #region Dependencies

        private readonly IRepository<WellnessCheckIn, Guid> _checkInRepository;
        private readonly IRepository<Hire, Guid> _hireRepository;

        #endregion

        public WellnessManager(
            IRepository<WellnessCheckIn, Guid> checkInRepository,
            IRepository<Hire, Guid> hireRepository)
        {
            _checkInRepository = checkInRepository;
            _hireRepository = hireRepository;
        }

        #region Public Methods

        /// <summary>
        /// Generates all 9 milestone WellnessCheckIn records for a journey.
        /// Each check-in is scheduled relative to the hire's StartDate.
        /// Called automatically when a journey is activated.
        /// </summary>
        public async Task GenerateCheckInsForJourneyAsync(Guid hireId, Guid journeyId)
        {
            var hire = await _hireRepository.GetAsync(hireId);
            Guard.Against.Null(hire, nameof(hire));

            var schedule = BuildCheckInSchedule(hire.StartDate);

            foreach (var (period, scheduledDate) in schedule)
            {
                var checkIn = BuildCheckIn(hireId, journeyId, period, scheduledDate);
                await _checkInRepository.InsertAsync(checkIn);
            }
        }

        #endregion

        #region Private Methods

        private List<(WellnessCheckInPeriod Period, DateTime ScheduledDate)> BuildCheckInSchedule(DateTime startDate)
        {
            return new List<(WellnessCheckInPeriod, DateTime)>
            {
                (WellnessCheckInPeriod.Day1,   startDate.AddDays(DAY1_OFFSET)),
                (WellnessCheckInPeriod.Day2,   startDate.AddDays(DAY2_OFFSET)),
                (WellnessCheckInPeriod.Week1,  startDate.AddDays(WEEK1_OFFSET)),
                (WellnessCheckInPeriod.Month1, startDate.AddDays(MONTH1_OFFSET)),
                (WellnessCheckInPeriod.Month2, startDate.AddDays(MONTH2_OFFSET)),
                (WellnessCheckInPeriod.Month3, startDate.AddDays(MONTH3_OFFSET)),
                (WellnessCheckInPeriod.Month4, startDate.AddDays(MONTH4_OFFSET)),
                (WellnessCheckInPeriod.Month5, startDate.AddDays(MONTH5_OFFSET)),
                (WellnessCheckInPeriod.Month6, startDate.AddDays(MONTH6_OFFSET))
            };
        }

        private WellnessCheckIn BuildCheckIn(
            Guid hireId,
            Guid journeyId,
            WellnessCheckInPeriod period,
            DateTime scheduledDate)
        {
            return new WellnessCheckIn
            {
                HireId        = hireId,
                JourneyId     = journeyId,
                Period        = period,
                ScheduledDate = scheduledDate,
                Status        = WellnessCheckInStatus.Pending
            };
        }

        #endregion
    }
}
