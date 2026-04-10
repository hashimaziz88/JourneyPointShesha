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
    /// Manages OnboardingDocuments uploaded to a plan for AI task extraction.
    /// Document creation and accepted proposal application delegate to OnboardingDocumentManager.
    /// </summary>
    public class OnboardingDocumentAppService : SheshaAppServiceBase
    {
        private readonly IRepository<OnboardingDocument, Guid> _documentRepository;
        private readonly IRepository<ExtractedTask, Guid> _extractedTaskRepository;
        private readonly OnboardingDocumentManager _documentManager;

        public OnboardingDocumentAppService(
            IRepository<OnboardingDocument, Guid> documentRepository,
            IRepository<ExtractedTask, Guid> extractedTaskRepository,
            OnboardingDocumentManager documentManager)
        {
            _documentRepository    = documentRepository;
            _extractedTaskRepository = extractedTaskRepository;
            _documentManager       = documentManager;
        }

        /// <summary>Returns all documents uploaded against a specific OnboardingPlan.</summary>
        [HttpGet, Route("[action]")]
        public async Task<List<DynamicDto<OnboardingDocument, Guid>>> GetPlanDocuments(Guid planId)
        {
            var docs = await _documentRepository.GetAllListAsync(d => d.OnboardingPlanId == planId);
            var result = new List<DynamicDto<OnboardingDocument, Guid>>();
            foreach (var doc in docs)
                result.Add(await MapToDynamicDtoAsync<OnboardingDocument, Guid>(doc));
            return result;
        }

        /// <summary>Returns full detail of a document including its extracted task proposals.</summary>
        [HttpGet, Route("[action]")]
        public async Task<DynamicDto<OnboardingDocument, Guid>> GetDetail(Guid id)
        {
            var doc = await _documentRepository.GetAsync(id);
            return await MapToDynamicDtoAsync<OnboardingDocument, Guid>(doc);
        }

        /// <summary>
        /// Creates an OnboardingDocument record for a file already stored by the caller.
        /// After creation, call StartExtraction to trigger AI task extraction.
        /// </summary>
        [HttpPost, Route("[action]")]
        public async Task<DynamicDto<OnboardingDocument, Guid>> Upload(
            Guid planId,
            string fileName,
            string storagePath,
            string contentType,
            long fileSizeBytes)
        {
            var document = await _documentManager.CreateDocumentAsync(planId, fileName, storagePath, contentType, fileSizeBytes);
            return await MapToDynamicDtoAsync<OnboardingDocument, Guid>(document);
        }

        /// <summary>Triggers asynchronous AI extraction of tasks from the document via GroqExtractionService.</summary>
        [HttpPost, Route("[action]")]
        public async Task StartExtraction(Guid documentId)
        {
            var doc = await _documentRepository.GetAsync(documentId);

            if (doc.Status != OnboardingDocumentStatus.Uploaded)
                throw new UserFriendlyException("Only Uploaded documents can be sent for extraction.");

            doc.Status = OnboardingDocumentStatus.Extracting;
            await _documentRepository.UpdateAsync(doc);

            // TODO: enqueue background job → GroqExtractionService.ExtractAsync(documentId)
        }

        /// <summary>Updates an AI-extracted task proposal before the Facilitator accepts or rejects it.</summary>
        [HttpPut, Route("[action]")]
        public async Task<DynamicDto<ExtractedTask, Guid>> UpdateProposal(UpdateProposalDto input)
        {
            var task = await _extractedTaskRepository.GetAsync(input.ExtractedTaskId);

            if (input.Title != null) task.Title = input.Title;
            if (input.Description != null) task.Description = input.Description;
            if (input.Category.HasValue) task.Category = input.Category.Value;
            if (input.AssignmentTarget.HasValue) task.AssignmentTarget = input.AssignmentTarget.Value;
            if (input.AcknowledgementRule.HasValue) task.AcknowledgementRule = input.AcknowledgementRule.Value;
            if (input.DueDayOffset.HasValue) task.DueDayOffset = input.DueDayOffset.Value;

            var updated = await _extractedTaskRepository.UpdateAsync(task);
            return await MapToDynamicDtoAsync<ExtractedTask, Guid>(updated);
        }

        /// <summary>Marks an extracted task proposal as Accepted for inclusion in the plan.</summary>
        [HttpPost, Route("[action]")]
        public async Task<DynamicDto<ExtractedTask, Guid>> AcceptProposal(Guid extractedTaskId)
        {
            var task = await _extractedTaskRepository.GetAsync(extractedTaskId);
            task.ReviewStatus    = ExtractedTaskReviewStatus.Accepted;
            task.ReviewedByUserId = AbpSession.UserId;
            task.ReviewedTime    = DateTime.UtcNow;
            var updated = await _extractedTaskRepository.UpdateAsync(task);
            return await MapToDynamicDtoAsync<ExtractedTask, Guid>(updated);
        }

        /// <summary>Marks an extracted task proposal as Rejected so it will not be added to the plan.</summary>
        [HttpPost, Route("[action]")]
        public async Task<DynamicDto<ExtractedTask, Guid>> RejectProposal(Guid extractedTaskId)
        {
            var task = await _extractedTaskRepository.GetAsync(extractedTaskId);
            task.ReviewStatus    = ExtractedTaskReviewStatus.Rejected;
            task.ReviewedByUserId = AbpSession.UserId;
            task.ReviewedTime    = DateTime.UtcNow;
            var updated = await _extractedTaskRepository.UpdateAsync(task);
            return await MapToDynamicDtoAsync<ExtractedTask, Guid>(updated);
        }

        /// <summary>
        /// Converts all Accepted proposals from a document into OnboardingTask records
        /// appended to the plan's last module.
        /// </summary>
        [HttpPost, Route("[action]")]
        public async Task ApplyAcceptedProposals(Guid documentId)
        {
            await _documentManager.ApplyAcceptedProposalsAsync(documentId);
        }
    }
}
