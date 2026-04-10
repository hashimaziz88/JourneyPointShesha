using Hashim.JourneyPoint.Common.Services.OnboardingPlans.Dtos;
using Hashim.JourneyPoint.Domain.Domain.OnboardingPlans;
using Microsoft.AspNetCore.Mvc;
using Shesha;
using Shesha.DynamicEntities.Dtos;
using System;
using System.Threading.Tasks;

namespace Hashim.JourneyPoint.Common.Services.OnboardingPlans
{
    /// <summary>
    /// Allows Facilitators to import an OnboardingPlan from structured markdown.
    /// The markdown format uses headings for modules and list items for tasks.
    /// Preview shows what will be created before the Facilitator commits.
    /// </summary>
    public class MarkdownImportAppService : SheshaAppServiceBase
    {
        /// <summary>
        /// Parses markdown content and returns a preview of the plan structure that would be created,
        /// without persisting anything to the database.
        /// </summary>
        [HttpPost, Route("[action]")]
        public async Task<object> Preview(MarkdownPreviewDto input)
        {
            // TODO: implement markdown parser → return preview DTO (modules + tasks)
            throw new NotImplementedException("Preview: parse markdown, return structured preview without saving");
        }

        /// <summary>
        /// Parses markdown content and persists it as a Draft OnboardingPlan with modules and tasks.
        /// </summary>
        [HttpPost, Route("[action]")]
        public async Task<DynamicDto<OnboardingPlan, Guid>> SaveDraft(SaveDraftFromMarkdownDto input)
        {
            // TODO: parse markdown, create OnboardingPlan + OnboardingModules + OnboardingTasks
            throw new NotImplementedException("SaveDraft: parse markdown, persist as Draft plan with modules and tasks");
        }
    }
}
