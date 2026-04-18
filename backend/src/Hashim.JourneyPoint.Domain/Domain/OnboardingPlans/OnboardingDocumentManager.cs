using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.UI;
using Ardalis.GuardClauses;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hashim.JourneyPoint.Domain.Domain.OnboardingPlans
{
    /// <summary>
    /// Orchestrates the OnboardingDocument lifecycle.
    /// Handles document registration and conversion of Accepted AI proposals
    /// into permanent OnboardingTask records on the plan.
    /// </summary>
    public class OnboardingDocumentManager : DomainService
    {
        #region Dependencies

        private readonly IRepository<OnboardingDocument, Guid> _documentRepository;
        private readonly IRepository<ExtractedTask, Guid> _extractedTaskRepository;
        private readonly IRepository<OnboardingPlan, Guid> _planRepository;
        private readonly IRepository<OnboardingModule, Guid> _moduleRepository;
        private readonly IRepository<OnboardingTask, Guid> _onboardingTaskRepository;

        #endregion

        public OnboardingDocumentManager(
            IRepository<OnboardingDocument, Guid> documentRepository,
            IRepository<ExtractedTask, Guid> extractedTaskRepository,
            IRepository<OnboardingPlan, Guid> planRepository,
            IRepository<OnboardingModule, Guid> moduleRepository,
            IRepository<OnboardingTask, Guid> onboardingTaskRepository)
        {
            _documentRepository = documentRepository;
            _extractedTaskRepository = extractedTaskRepository;
            _planRepository = planRepository;
            _moduleRepository = moduleRepository;
            _onboardingTaskRepository = onboardingTaskRepository;
        }

        #region Public Methods

        /// <summary>
        /// Registers an uploaded file as an OnboardingDocument in Uploaded status
        /// ready for AI extraction.
        /// </summary>
        public async Task<OnboardingDocument> CreateDocumentAsync(
            Guid planId,
            string fileName,
            string storagePath,
            string contentType,
            long fileSizeBytes)
        {
            Guard.Against.NullOrWhiteSpace(fileName, nameof(fileName));
            Guard.Against.NullOrWhiteSpace(storagePath, nameof(storagePath));
            Guard.Against.NullOrWhiteSpace(contentType, nameof(contentType));
            Guard.Against.NegativeOrZero(fileSizeBytes, nameof(fileSizeBytes));

            var plan = await _planRepository.GetAsync(planId);
            Guard.Against.Null(plan, nameof(plan));

            var document = new OnboardingDocument
            {
                OnboardingPlanId = planId,
                FileName         = fileName,
                StoragePath      = storagePath,
                ContentType      = contentType,
                FileSizeBytes    = fileSizeBytes,
                Status           = OnboardingDocumentStatus.Uploaded
            };
            return await _documentRepository.InsertAsync(document);
        }

        /// <summary>
        /// Converts all Accepted proposals from a document into OnboardingTask records
        /// appended to the plan's last module, then marks the document as Applied.
        /// </summary>
        public async Task ApplyAcceptedProposalsAsync(Guid documentId)
        {
            var document = await _documentRepository.GetAsync(documentId);
            Guard.Against.Null(document, nameof(document));

            var lastModule = await GetLastModuleAsync(document.OnboardingPlanId);
            if (lastModule == null)
                throw new UserFriendlyException("The plan has no modules. Add at least one module before applying proposals.");

            var accepted = await _extractedTaskRepository.GetAllListAsync(
                t => t.OnboardingDocumentId == documentId
                  && t.ReviewStatus == ExtractedTaskReviewStatus.Accepted);

            var maxOrderIndex = await GetMaxTaskOrderIndexAsync(lastModule.Id);
            var orderCounter  = maxOrderIndex;

            foreach (var proposal in accepted)
            {
                orderCounter++;
                var task = await InsertTaskFromProposalAsync(proposal, lastModule, orderCounter);
                proposal.ReviewStatus          = ExtractedTaskReviewStatus.Applied;
                proposal.AppliedOnboardingTaskId = task.Id;
                await _extractedTaskRepository.UpdateAsync(proposal);
            }

            document.AcceptedTaskCount = accepted.Count;
            document.Status            = OnboardingDocumentStatus.Applied;
            await _documentRepository.UpdateAsync(document);
        }

        #endregion

        #region Private Methods

        private async Task<OnboardingModule> GetLastModuleAsync(Guid planId)
        {
            var modules = await _moduleRepository.GetAllListAsync(m => m.OnboardingPlanId == planId);
            return modules.OrderByDescending(m => m.OrderIndex).FirstOrDefault();
        }

        private async Task<int> GetMaxTaskOrderIndexAsync(Guid moduleId)
        {
            var tasks = await _onboardingTaskRepository.GetAllListAsync(t => t.OnboardingModuleId == moduleId);
            return tasks.Any() ? tasks.Max(t => t.OrderIndex) : 0;
        }

        private async Task<OnboardingTask> InsertTaskFromProposalAsync(ExtractedTask proposal, OnboardingModule module, int orderIndex)
        {
            var task = new OnboardingTask
            {
                OnboardingModuleId  = module.Id,
                Title               = proposal.Title,
                Description         = proposal.Description,
                Category            = proposal.Category,
                DueDayOffset        = proposal.DueDayOffset,
                AssignmentTarget    = proposal.AssignmentTarget,
                AcknowledgementRule = proposal.AcknowledgementRule,
                OrderIndex          = orderIndex
            };
            return await _onboardingTaskRepository.InsertAsync(task);
        }

        #endregion
    }
}
