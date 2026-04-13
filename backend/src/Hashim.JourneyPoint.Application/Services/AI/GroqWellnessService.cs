using Abp.Dependency;
using Abp.Domain.Repositories;
using Castle.Core.Logging;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Hashim.JourneyPoint.Domain.Domain.Hires;
using Hashim.JourneyPoint.Domain.Domain.Wellness;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hashim.JourneyPoint.Common.Services.AI
{
    /// <summary>
    /// Generates AI-authored wellness check-in questions for all 9 milestone periods
    /// of a hire's journey. Called automatically at hire creation — one Groq call
    /// produces all 45 questions (5 per period) and persists them as WellnessQuestion records.
    /// Questions are personalised to the hire's role, department, and start date.
    /// </summary>
    public class GroqWellnessService : ITransientDependency
    {
        #region Constants

        private const string GROQ_API_URL     = "https://api.groq.com/openai/v1/chat/completions";
        private const int    QUESTIONS_PER_CHECK_IN = 5;

        private const string SYSTEM_PROMPT =
            "You are an empathetic HR onboarding specialist generating personalised wellness check-in questions.\n" +
            "Given a new hire's profile, generate exactly 5 open-ended questions for each of the 9 milestone periods.\n" +
            "Questions should be warm, reflective, and appropriate for the timing in the onboarding journey.\n" +
            "Return ONLY valid JSON (no markdown, no code fences) using this exact schema:\n" +
            "{\n" +
            "  \"checkIns\": [\n" +
            "    { \"period\": \"Day1\",   \"questions\": [\"...\",\"...\",\"...\",\"...\",\"...\"] },\n" +
            "    { \"period\": \"Day2\",   \"questions\": [\"...\",\"...\",\"...\",\"...\",\"...\"] },\n" +
            "    { \"period\": \"Week1\",  \"questions\": [\"...\",\"...\",\"...\",\"...\",\"...\"] },\n" +
            "    { \"period\": \"Month1\", \"questions\": [\"...\",\"...\",\"...\",\"...\",\"...\"] },\n" +
            "    { \"period\": \"Month2\", \"questions\": [\"...\",\"...\",\"...\",\"...\",\"...\"] },\n" +
            "    { \"period\": \"Month3\", \"questions\": [\"...\",\"...\",\"...\",\"...\",\"...\"] },\n" +
            "    { \"period\": \"Month4\", \"questions\": [\"...\",\"...\",\"...\",\"...\",\"...\"] },\n" +
            "    { \"period\": \"Month5\", \"questions\": [\"...\",\"...\",\"...\",\"...\",\"...\"] },\n" +
            "    { \"period\": \"Month6\", \"questions\": [\"...\",\"...\",\"...\",\"...\",\"...\"] }\n" +
            "  ]\n" +
            "}\n" +
            "Period context:\n" +
            "Day1=first day (first impressions, nerves, excitement). " +
            "Day2=second day (settling in, early observations). " +
            "Week1=end of first week (team integration, comfort). " +
            "Month1=first month complete (role clarity, early wins). " +
            "Month2=deepening integration, challenges. " +
            "Month3=quarter review, belonging, confidence. " +
            "Month4=growing independence, skills. " +
            "Month5=building momentum. " +
            "Month6=half-year milestone, reflection, future goals.";

        #endregion

        #region Dependencies

        private static readonly HttpClient            _http            = new();
        private static readonly JsonSerializerOptions _caseInsensitive = new() { PropertyNameCaseInsensitive = true };
        private static readonly JsonSerializerOptions _camelCase       = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        private readonly IRepository<WellnessCheckIn,  Guid> _checkInRepository;
        private readonly IRepository<WellnessQuestion, Guid> _questionRepository;
        private readonly IRepository<Hire,             Guid> _hireRepository;
        private readonly IConfiguration                       _config;

        /// <summary>ABP property-injected logger. Falls back to NullLogger if not set.</summary>
        public ILogger Logger { get; set; } = NullLogger.Instance;

        #endregion

        public GroqWellnessService(
            IRepository<WellnessCheckIn,  Guid> checkInRepository,
            IRepository<WellnessQuestion, Guid> questionRepository,
            IRepository<Hire,             Guid> hireRepository,
            IConfiguration config)
        {
            _checkInRepository  = checkInRepository;
            _questionRepository = questionRepository;
            _hireRepository     = hireRepository;
            _config             = config;
        }

        #region Public Methods

        /// <summary>
        /// Generates and persists 5 wellness questions for each of the 9 check-in periods
        /// belonging to the given journey. Makes a single Groq call.
        /// Swallows Groq failures — hire creation is not blocked by AI errors.
        /// </summary>
        public async Task GenerateQuestionsForJourneyAsync(Guid hireId, Guid journeyId)
        {
            try
            {
                var hire     = await _hireRepository.GetAsync(hireId);
                var checkIns = await _checkInRepository.GetAllListAsync(c => c.JourneyId == journeyId);

                if (!checkIns.Any()) return;

                var extraction = await CallGroqAsync(hire);
                if (extraction?.CheckIns == null) return;

                await PersistQuestionsAsync(extraction.CheckIns, checkIns);
            }
            catch (Exception ex)
            {
                Logger.Error($"GroqWellnessService failed for journeyId={journeyId}. Questions not generated.", ex);
            }
        }

        #endregion

        #region Private Methods

        private async Task<WellnessGroqResponse?> CallGroqAsync(Hire hire)
        {
            var apiKey = _config["Groq:ApiKey"];
            var model  = _config["Groq:Model"] ?? "llama-3.3-70b-versatile";

            var userMessage =
                $"Hire profile:\n" +
                $"- Name: {hire.FullName}\n" +
                $"- Role: {hire.RoleTitle}\n" +
                $"- Department: {hire.Department}\n" +
                $"- Start Date: {hire.StartDate:yyyy-MM-dd}\n\n" +
                $"Generate personalised wellness check-in questions for all 9 milestone periods.";

            var payload = new
            {
                model,
                response_format = new { type = "json_object" },
                messages = new[]
                {
                    new { role = "system", content = SYSTEM_PROMPT },
                    new { role = "user",   content = userMessage   }
                }
            };

            var json    = JsonSerializer.Serialize(payload, _camelCase);
            var request = new HttpRequestMessage(HttpMethod.Post, GROQ_API_URL)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            using var response = await _http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Groq API error {(int)response.StatusCode}: {body}");

            var envelope   = JsonSerializer.Deserialize<GroqApiEnvelope>(body, _caseInsensitive);
            var rawContent = envelope?.Choices?[0]?.Message?.Content
                             ?? throw new Exception("Groq returned an empty response.");

            return JsonSerializer.Deserialize<WellnessGroqResponse>(rawContent, _caseInsensitive);
        }

        private async Task PersistQuestionsAsync(
            List<WellnessPeriodData> periods,
            List<WellnessCheckIn>    checkIns)
        {
            var checkInByPeriod = checkIns.ToDictionary(c => c.Period.ToString(), c => c);

            foreach (var period in periods)
            {
                if (!checkInByPeriod.TryGetValue(period.Period ?? "", out var checkIn)) continue;
                if (period.Questions == null) continue;

                // Replace the default questions seeded by WellnessManager with AI-personalised ones.
                var existing = await _questionRepository.GetAllListAsync(q => q.WellnessCheckInId == checkIn.Id);
                var byIndex  = existing.ToDictionary(q => q.OrderIndex);

                for (int i = 0; i < period.Questions.Count; i++)
                {
                    var orderIndex = i + 1;
                    if (byIndex.TryGetValue(orderIndex, out var existing_q))
                    {
                        existing_q.QuestionText = period.Questions[i];
                        await _questionRepository.UpdateAsync(existing_q);
                    }
                    else
                    {
                        await _questionRepository.InsertAsync(new WellnessQuestion
                        {
                            WellnessCheckInId = checkIn.Id,
                            OrderIndex        = orderIndex,
                            QuestionText      = period.Questions[i],
                            IsAnswered        = false
                        });
                    }
                }
            }
        }

        #endregion

        #region Internal Groq DTOs

        private class WellnessGroqResponse
        {
            [JsonPropertyName("checkIns")]
            public List<WellnessPeriodData>? CheckIns { get; set; }
        }

        private class WellnessPeriodData
        {
            [JsonPropertyName("period")]
            public string? Period { get; set; }

            [JsonPropertyName("questions")]
            public List<string>? Questions { get; set; }
        }

        private class GroqApiEnvelope
        {
            [JsonPropertyName("choices")]
            public List<GroqChoice>? Choices { get; set; }
        }

        private class GroqChoice
        {
            [JsonPropertyName("message")]
            public GroqMessage? Message { get; set; }
        }

        private class GroqMessage
        {
            [JsonPropertyName("content")]
            public string? Content { get; set; }
        }

        #endregion
    }
}
