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
    /// (Day1 through Month6). Each check-in is seeded with 5 default questions immediately;
    /// GroqWellnessService may later replace these with personalised content.
    /// Submission validation enforces that all questions are answered.
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

        private readonly IRepository<WellnessCheckIn,  Guid> _checkInRepository;
        private readonly IRepository<WellnessQuestion, Guid> _questionRepository;
        private readonly IRepository<Hire,             Guid> _hireRepository;

        #endregion

        public WellnessManager(
            IRepository<WellnessCheckIn,  Guid> checkInRepository,
            IRepository<WellnessQuestion, Guid> questionRepository,
            IRepository<Hire,             Guid> hireRepository)
        {
            _checkInRepository  = checkInRepository;
            _questionRepository = questionRepository;
            _hireRepository     = hireRepository;
        }

        #region Public Methods

        /// <summary>
        /// Generates all 9 milestone WellnessCheckIn records for a journey,
        /// each seeded with 5 default questions. Questions are always created here
        /// so the hire always has something to answer even if AI personalisation fails.
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
                var saved   = await _checkInRepository.InsertAsync(checkIn);

                await SeedDefaultQuestionsAsync(saved.Id, period);
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

        private async Task SeedDefaultQuestionsAsync(Guid checkInId, WellnessCheckInPeriod period)
        {
            var questions = GetDefaultQuestions(period);
            for (int i = 0; i < questions.Count; i++)
            {
                await _questionRepository.InsertAsync(new WellnessQuestion
                {
                    WellnessCheckInId = checkInId,
                    OrderIndex        = i + 1,
                    QuestionText      = questions[i],
                    IsAnswered        = false
                });
            }
        }

        private static List<string> GetDefaultQuestions(WellnessCheckInPeriod period) => period switch
        {
            WellnessCheckInPeriod.Day1 => new List<string>
            {
                "How are you feeling on your very first day?",
                "What has surprised you most so far?",
                "Do you feel welcomed by your team?",
                "Do you have everything you need to get started?",
                "What are you most looking forward to in this role?"
            },
            WellnessCheckInPeriod.Day2 => new List<string>
            {
                "How does your second day compare to your first impressions?",
                "Are you getting comfortable with your workspace and tools?",
                "Have you had a chance to connect with your key colleagues?",
                "Is there anything that has felt unclear or confusing?",
                "How does the team culture feel so far?"
            },
            WellnessCheckInPeriod.Week1 => new List<string>
            {
                "How has your first week been overall?",
                "Do you feel clear on what is expected of you in your role?",
                "Have you encountered any challenges you would like support with?",
                "How well are you settling into the team dynamic?",
                "What has been a highlight from your first week?"
            },
            WellnessCheckInPeriod.Month1 => new List<string>
            {
                "How are you feeling after your first full month?",
                "Do you have a clear understanding of your role responsibilities?",
                "How supported do you feel by your manager and colleagues?",
                "What is going well for you so far?",
                "Is there anything we can do to improve your onboarding experience?"
            },
            WellnessCheckInPeriod.Month2 => new List<string>
            {
                "How is your confidence growing in your role?",
                "Are there any skills or knowledge gaps you would like to address?",
                "How is your relationship with your team developing?",
                "What has been your biggest achievement this month?",
                "Is there anything making your work more difficult than expected?"
            },
            WellnessCheckInPeriod.Month3 => new List<string>
            {
                "Reflecting on your first three months, how do you feel you have settled in?",
                "Do you feel a sense of belonging within the organisation?",
                "How aligned do you feel with the team's goals and priorities?",
                "What feedback have you received that has been most valuable?",
                "What would you like to focus on in the coming months?"
            },
            WellnessCheckInPeriod.Month4 => new List<string>
            {
                "How independently are you able to perform your core responsibilities?",
                "What new skills or knowledge have you developed?",
                "How comfortable are you raising concerns or ideas with your manager?",
                "What aspects of the job energise you the most?",
                "Is there any additional support or development you would benefit from?"
            },
            WellnessCheckInPeriod.Month5 => new List<string>
            {
                "How would you describe your overall experience so far?",
                "What momentum are you building in your role?",
                "How well do you collaborate with cross-functional teams?",
                "What impact do you feel you are making?",
                "Are there areas where you feel you still need more guidance?"
            },
            WellnessCheckInPeriod.Month6 => new List<string>
            {
                "As you reach the six-month milestone, how settled do you feel?",
                "Looking back, what have been your proudest achievements during onboarding?",
                "What aspects of the organisation's culture resonate most with you?",
                "What are your goals for the next six months?",
                "What would you change about your onboarding experience if you could?"
            },
            _ => new List<string>
            {
                "How are you feeling at this stage of your onboarding?",
                "What is going well for you?",
                "Are there any challenges you would like support with?",
                "What has been your biggest learning this period?",
                "Is there anything you would like your facilitator to know?"
            }
        };

        #endregion
    }
}
