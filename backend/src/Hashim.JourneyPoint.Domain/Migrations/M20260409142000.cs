using FluentMigrator;
using Shesha.FluentMigrator;

namespace Hashim.JourneyPoint.Domain.Migrations
{
    /// <summary>
    /// Creates OnboardingDocument and ExtractedTask tables.
    /// Documents store metadata for files uploaded via Shesha's file management.
    /// Depends on M20260409140000 (OnboardingPlans must exist).
    /// </summary>
    [Migration(20260409142000)]
    public class M20260409142000 : OneWayMigration
    {
        public override void Up()
        {
            // OnboardingDocuments
            Create.Table("JourneyPoint_OnboardingDocuments")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithColumn("TenantId").AsInt32().WithDefaultValue(1)
                .WithForeignKeyColumn("OnboardingPlanId", "JourneyPoint_OnboardingPlans").Nullable()
                .WithColumn("FileName").AsString(260).NotNullable()
                .WithColumn("StoragePath").AsString(500).NotNullable()
                .WithColumn("ContentType").AsString(200).NotNullable()
                .WithColumn("FileSizeBytes").AsInt64().WithDefaultValue(0)
                .WithColumn("StatusLkp").AsInt64().Nullable()
                .WithColumn("ExtractedTaskCount").AsInt32().WithDefaultValue(0)
                .WithColumn("AcceptedTaskCount").AsInt32().WithDefaultValue(0)
                .WithColumn("FailureReason").AsString(2000).Nullable()
                .WithColumn("ExtractionCompletedTime").AsDateTime().Nullable();

            // ExtractedTasks
            Create.Table("JourneyPoint_ExtractedTasks")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithColumn("TenantId").AsInt32().WithDefaultValue(1)
                .WithForeignKeyColumn("OnboardingDocumentId", "JourneyPoint_OnboardingDocuments").Nullable()
                .WithColumn("SuggestedModuleId").AsGuid().Nullable()
                .WithColumn("Title").AsString(200).NotNullable()
                .WithColumn("Description").AsString(int.MaxValue).Nullable()
                .WithColumn("CategoryLkp").AsInt64().Nullable()
                .WithColumn("AssignmentTargetLkp").AsInt64().Nullable()
                .WithColumn("AcknowledgementRuleLkp").AsInt64().Nullable()
                .WithColumn("DueDayOffset").AsInt32().WithDefaultValue(0)
                .WithColumn("ReviewStatusLkp").AsInt64().Nullable()
                .WithColumn("ReviewedByUserId").AsInt64().Nullable()
                .WithColumn("ReviewedTime").AsDateTime().Nullable()
                .WithColumn("AppliedOnboardingTaskId").AsGuid().Nullable();
        }
    }
}
