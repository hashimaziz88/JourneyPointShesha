using Abp.Domain.Repositories;
using Abp.UI;
using Hashim.JourneyPoint.Common.Services.OnboardingPlans.Dtos;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Hashim.JourneyPoint.Domain.Domain.OnboardingPlans;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PdfSharpCore.Pdf.Content;
using PdfSharpCore.Pdf.Content.Objects;
using PdfSharpCore.Pdf.IO;
using Shesha;
using Shesha.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hashim.JourneyPoint.Common.Services.OnboardingPlans
{
    /// <summary>
    /// Accepts a Shesha StoredFile ID (PDF, markdown, or plain text), sends its content to
    /// the Groq LLM, and creates a new OnboardingPlan with modules and tasks directly from
    /// the extracted content. No intermediate review step.
    ///
    /// Upload flow:
    ///   1. Frontend calls POST /api/StoredFile/Upload — Shesha returns a StoredFile ID.
    ///   2. Frontend calls POST .../ImportDocument?storedFileId={id}.
    ///   3. Returns the new plan ID for immediate navigation.
    /// </summary>
    [Route("api/services/app/GroqPlanImport/[action]")]
    public class GroqPlanImportAppService : SheshaAppServiceBase
    {
        #region Constants

        private const string GROQ_API_URL     = "https://api.groq.com/openai/v1/chat/completions";
        private const int    MAX_DOCUMENT_CHARS = 12000;

        private const string SYSTEM_PROMPT =
            "You are an HR onboarding document analyser. Extract a complete onboarding plan from the provided document.\n" +
            "Return ONLY valid JSON (no markdown, no code fences). Use this exact schema:\n" +
            "{\n" +
            "  \"planName\": \"...\",\n" +
            "  \"planDescription\": \"...\",\n" +
            "  \"targetAudience\": \"...\",\n" +
            "  \"durationDays\": 30,\n" +
            "  \"modules\": [\n" +
            "    {\n" +
            "      \"name\": \"...\", \"description\": \"...\", \"orderIndex\": 1,\n" +
            "      \"tasks\": [\n" +
            "        { \"title\": \"...\", \"description\": \"...\", \"orderIndex\": 1,\n" +
            "          \"dueDayOffset\": 1, \"category\": 1, \"assignmentTarget\": 1, \"acknowledgementRule\": 1 }\n" +
            "      ]\n" +
            "    }\n" +
            "  ]\n" +
            "}\n" +
            "Enum values — category: 1=Orientation 2=Learning 3=Practice 4=Assessment 5=CheckIn. " +
            "assignmentTarget: 1=Enrolee 2=Manager 3=Facilitator. " +
            "acknowledgementRule: 1=NotRequired 2=Required. " +
            "Default to category=1, assignmentTarget=1, acknowledgementRule=1 if not clear from context. " +
            "dueDayOffset = calendar days from hire start date when the task is due. " +
            "durationDays = total expected onboarding duration in calendar days.";

        #endregion

        #region Dependencies

        private static readonly HttpClient           _http            = new();
        private static readonly JsonSerializerOptions _camelCase       = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private static readonly JsonSerializerOptions _caseInsensitive = new() { PropertyNameCaseInsensitive = true };

        private readonly IRepository<OnboardingPlan,   Guid> _planRepository;
        private readonly IRepository<OnboardingModule, Guid> _moduleRepository;
        private readonly IRepository<OnboardingTask,   Guid> _taskRepository;
        private readonly IStoredFileService                   _storedFileService;
        private readonly IConfiguration                       _config;

        public GroqPlanImportAppService(
            IRepository<OnboardingPlan,   Guid> planRepository,
            IRepository<OnboardingModule, Guid> moduleRepository,
            IRepository<OnboardingTask,   Guid> taskRepository,
            IStoredFileService storedFileService,
            IConfiguration config)
        {
            _planRepository    = planRepository;
            _moduleRepository  = moduleRepository;
            _taskRepository    = taskRepository;
            _storedFileService = storedFileService;
            _config            = config;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reads a previously uploaded PDF, markdown, or text document, extracts a full
        /// onboarding plan via Groq, and persists a new OnboardingPlan with modules and tasks.
        /// Returns the new plan ID so the caller can navigate directly to the details page.
        /// </summary>
        /// <param name="storedFileId">ID returned by POST /api/StoredFile/Upload.</param>
        [HttpPost]
        public async Task<ImportDocumentResultDto> ImportDocument(Guid storedFileId)
        {
            if (storedFileId == Guid.Empty)
                throw new UserFriendlyException("storedFileId is required.");

            var apiKey = _config["Groq:ApiKey"];
            var model  = _config["Groq:Model"] ?? "llama-3.3-70b-versatile";

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new UserFriendlyException("Groq:ApiKey is not configured in appsettings.json.");

            var documentText = await ReadFileTextAsync(storedFileId);
            var extracted    = await ExtractFromGroqAsync(apiKey, model, documentText);

            return await PersistToDbAsync(extracted);
        }

        #endregion

        #region Private — File Reading

        private async Task<string> ReadFileTextAsync(Guid storedFileId)
        {
            var file = await _storedFileService.GetOrNullAsync(storedFileId);
            if (file == null)
                throw new UserFriendlyException($"StoredFile {storedFileId} not found.");

            using var stream = await _storedFileService.GetStreamAsync(file);

            var isPdf = ".pdf".Equals(file.FileType, StringComparison.OrdinalIgnoreCase);
            var text  = isPdf ? ExtractPdfText(stream) : await ReadUtf8TextAsync(stream);

            return text.Length > MAX_DOCUMENT_CHARS ? text[..MAX_DOCUMENT_CHARS] : text;
        }

        private static async Task<string> ReadUtf8TextAsync(Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }

        /// <summary>
        /// Extracts visible text from a PDF by iterating content-stream operators.
        /// Handles Tj (single string), TJ (array with kerning), ' and " operators.
        /// </summary>
        private static string ExtractPdfText(Stream stream)
        {
            var sb  = new StringBuilder();
            var doc = PdfReader.Open(stream, PdfDocumentOpenMode.ReadOnly);

            foreach (var page in doc.Pages)
            {
                AppendTextFromSequence(ContentReader.ReadContent(page), sb);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static void AppendTextFromSequence(CSequence sequence, StringBuilder sb)
        {
            foreach (var obj in sequence)
            {
                if (obj is COperator op && IsTextOperator(op.OpCode.Name))
                {
                    foreach (var operand in op.Operands)
                    {
                        if (operand is CString str)
                            sb.Append(str.Value).Append(' ');
                        else if (operand is CArray arr)
                            foreach (var item in arr)
                                if (item is CString s) sb.Append(s.Value);
                    }
                }
                else if (obj is CSequence nested)
                {
                    AppendTextFromSequence(nested, sb);
                }
            }
        }

        private static bool IsTextOperator(string name) => name is "Tj" or "TJ" or "'" or "\"";

        #endregion

        #region Private — Groq

        private async Task<GroqExtractionResult> ExtractFromGroqAsync(
            string apiKey, string model, string documentText)
        {
            var requestBody = new
            {
                model,
                messages = new[]
                {
                    new { role = "system", content = SYSTEM_PROMPT },
                    new { role = "user",   content = documentText }
                },
                temperature     = 0.3,
                response_format = new { type = "json_object" }
            };

            var requestJson = JsonSerializer.Serialize(requestBody, _camelCase);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, GROQ_API_URL);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            httpRequest.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new UserFriendlyException($"Groq API error ({(int)response.StatusCode}): {body}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            using var doc    = JsonDocument.Parse(responseJson);
            var contentStr   = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(contentStr))
                throw new UserFriendlyException("Groq returned an empty response. Try a different document.");

            var result = JsonSerializer.Deserialize<GroqExtractionResult>(contentStr, _caseInsensitive);

            if (result == null || string.IsNullOrWhiteSpace(result.PlanName))
                throw new UserFriendlyException(
                    "Groq could not extract a plan from the document. " +
                    "Ensure the document contains structured onboarding content.");

            return result;
        }

        #endregion

        #region Private — DB Persistence

        private async Task<ImportDocumentResultDto> PersistToDbAsync(GroqExtractionResult extracted)
        {
            var plan = await _planRepository.InsertAsync(new OnboardingPlan
            {
                Name           = extracted.PlanName.Length > 200
                                     ? extracted.PlanName[..200]
                                     : extracted.PlanName,
                Description    = extracted.PlanDescription ?? string.Empty,
                TargetAudience = extracted.TargetAudience  ?? string.Empty,
                DurationDays   = extracted.DurationDays > 0 ? extracted.DurationDays : 30,
                Status         = OnboardingPlanStatus.Draft
            });

            var result = new ImportDocumentResultDto
            {
                PlanId   = plan.Id,
                PlanName = plan.Name
            };

            foreach (var moduleData in extracted.Modules ?? new List<GroqModuleData>())
            {
                var module = await _moduleRepository.InsertAsync(new OnboardingModule
                {
                    OnboardingPlanId = plan.Id,
                    Name             = moduleData.Name        ?? "Unnamed Module",
                    Description      = moduleData.Description ?? string.Empty,
                    OrderIndex       = moduleData.OrderIndex
                });

                var taskCount = 0;

                foreach (var taskData in moduleData.Tasks ?? new List<GroqTaskData>())
                {
                    await _taskRepository.InsertAsync(new OnboardingTask
                    {
                        OnboardingModuleId  = module.Id,
                        Title               = taskData.Title       ?? "Unnamed Task",
                        Description         = taskData.Description ?? string.Empty,
                        OrderIndex          = taskData.OrderIndex,
                        DueDayOffset        = taskData.DueDayOffset,
                        Category            = IsValidCategory(taskData.Category)
                                                  ? (OnboardingTaskCategory)taskData.Category
                                                  : OnboardingTaskCategory.Orientation,
                        AssignmentTarget    = IsValidAssignmentTarget(taskData.AssignmentTarget)
                                                  ? (OnboardingTaskAssignmentTarget)taskData.AssignmentTarget
                                                  : OnboardingTaskAssignmentTarget.Enrolee,
                        AcknowledgementRule = IsValidAcknowledgementRule(taskData.AcknowledgementRule)
                                                  ? (OnboardingTaskAcknowledgementRule)taskData.AcknowledgementRule
                                                  : OnboardingTaskAcknowledgementRule.NotRequired
                    });

                    taskCount++;
                }

                result.Modules.Add(new CreatedModuleSummaryDto
                {
                    Id        = module.Id,
                    Name      = module.Name,
                    TaskCount = taskCount
                });

                result.ModulesCreated++;
                result.TasksCreated += taskCount;
            }

            return result;
        }

        private static bool IsValidCategory(long v)            => v >= 1 && v <= 5;
        private static bool IsValidAssignmentTarget(long v)    => v >= 1 && v <= 3;
        private static bool IsValidAcknowledgementRule(long v) => v >= 1 && v <= 2;

        #endregion
    }

    #region Internal Groq DTOs

    internal class GroqExtractionResult
    {
        [JsonPropertyName("planName")]        public string?          PlanName        { get; set; }
        [JsonPropertyName("planDescription")] public string?          PlanDescription { get; set; }
        [JsonPropertyName("targetAudience")]  public string?          TargetAudience  { get; set; }
        [JsonPropertyName("durationDays")]    public int              DurationDays    { get; set; }
        [JsonPropertyName("modules")]         public List<GroqModuleData>? Modules    { get; set; }
    }

    internal class GroqModuleData
    {
        [JsonPropertyName("name")]        public string?             Name        { get; set; }
        [JsonPropertyName("description")] public string?             Description { get; set; }
        [JsonPropertyName("orderIndex")]  public int                 OrderIndex  { get; set; }
        [JsonPropertyName("tasks")]       public List<GroqTaskData>? Tasks       { get; set; }
    }

    internal class GroqTaskData
    {
        [JsonPropertyName("title")]               public string? Title               { get; set; }
        [JsonPropertyName("description")]         public string? Description         { get; set; }
        [JsonPropertyName("orderIndex")]          public int     OrderIndex          { get; set; }
        [JsonPropertyName("dueDayOffset")]        public int     DueDayOffset        { get; set; }
        [JsonPropertyName("category")]            public long    Category            { get; set; }
        [JsonPropertyName("assignmentTarget")]    public long    AssignmentTarget    { get; set; }
        [JsonPropertyName("acknowledgementRule")] public long    AcknowledgementRule { get; set; }
    }

    #endregion
}
