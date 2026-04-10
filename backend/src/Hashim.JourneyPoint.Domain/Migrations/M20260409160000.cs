using FluentMigrator;
using Shesha.FluentMigrator;

namespace Hashim.JourneyPoint.Domain.Migrations
{
    /// <summary>
    /// Drops and recreates all JourneyPoint domain tables.
    /// The original migrations (M140000–M145000) created TenantId as a plain INT column
    /// with no FK constraint to AbpTenants. This migration corrects that by dropping all
    /// tables in dependency order (children first) and recreating them using
    /// Shesha's WithTenantIdAsRequired() helper, which adds the proper FK to AbpTenants
    /// and ensures ABP's multi-tenancy data filter functions correctly at runtime.
    /// All DROP statements use IF OBJECT_ID guards so this is safe to run on any environment.
    /// </summary>
    [Migration(20260409160000)]
    public class M20260409160000 : OneWayMigration
    {
        public override void Up()
        {
            DropAll();
            CreateAll();
        }

        #region Drop (children first, then parents)

        private void DropAll()
        {
            // Leaf tables — no other JP table references these
            DropIfExists("JourneyPoint_GenerationLogs");
            DropIfExists("JourneyPoint_WellnessQuestions");
            DropIfExists("JourneyPoint_ExtractedTasks");
            DropIfExists("JourneyPoint_JourneyTasks");
            DropIfExists("JourneyPoint_EngagementSnapshots");
            DropIfExists("JourneyPoint_AtRiskFlags");
            DropIfExists("JourneyPoint_OnboardingTasks");

            // Mid-level — their children are already gone
            DropIfExists("JourneyPoint_WellnessCheckIns");
            DropIfExists("JourneyPoint_OnboardingDocuments");
            DropIfExists("JourneyPoint_OnboardingModules");
            DropIfExists("JourneyPoint_Journeys");
            DropIfExists("JourneyPoint_Hires");

            // Root table — all dependants removed above
            DropIfExists("JourneyPoint_OnboardingPlans");
        }

        private void DropIfExists(string table)
        {
            Execute.Sql($@"
                IF OBJECT_ID(N'[dbo].[{table}]', N'U') IS NOT NULL
                    DROP TABLE [dbo].[{table}];
            ");
        }

        #endregion

        #region Create (parents first, then children)

        private void CreateAll()
        {
            CreateOnboardingPlans();
            CreateOnboardingModules();
            CreateOnboardingTasks();
            CreateHires();
            CreateJourneys();
            CreateJourneyTasks();
            CreateOnboardingDocuments();
            CreateExtractedTasks();
            CreateWellnessCheckIns();
            CreateWellnessQuestions();
            CreateEngagementSnapshots();
            CreateAtRiskFlags();
            CreateGenerationLogs();
        }

        private void CreateOnboardingPlans()
        {
            Create.Table("JourneyPoint_OnboardingPlans")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithTenantIdAsRequired()
                .WithColumn("Name").AsString(200).NotNullable()
                .WithColumn("Description").AsString(int.MaxValue).Nullable()
                .WithColumn("TargetAudience").AsString(200).Nullable()
                .WithColumn("DurationDays").AsInt32().WithDefaultValue(0)
                .WithColumn("StatusLkp").AsInt64().Nullable();
        }

        private void CreateOnboardingModules()
        {
            Create.Table("JourneyPoint_OnboardingModules")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithTenantIdAsRequired()
                .WithForeignKeyColumn("OnboardingPlanId", "JourneyPoint_OnboardingPlans").Nullable()
                .WithColumn("Name").AsString(200).NotNullable()
                .WithColumn("Description").AsString(int.MaxValue).Nullable()
                .WithColumn("OrderIndex").AsInt32().WithDefaultValue(0)
                .WithColumn("DurationDays").AsInt32().WithDefaultValue(0);
        }

        private void CreateOnboardingTasks()
        {
            Create.Table("JourneyPoint_OnboardingTasks")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithTenantIdAsRequired()
                .WithForeignKeyColumn("OnboardingModuleId", "JourneyPoint_OnboardingModules").Nullable()
                .WithColumn("Title").AsString(200).NotNullable()
                .WithColumn("Description").AsString(int.MaxValue).Nullable()
                .WithColumn("CategoryLkp").AsInt64().Nullable()
                .WithColumn("AssignmentTargetLkp").AsInt64().Nullable()
                .WithColumn("AcknowledgementRuleLkp").AsInt64().Nullable()
                .WithColumn("DueDayOffset").AsInt32().WithDefaultValue(0)
                .WithColumn("OrderIndex").AsInt32().WithDefaultValue(0);
        }

        private void CreateHires()
        {
            Create.Table("JourneyPoint_Hires")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithTenantIdAsRequired()
                .WithForeignKeyColumn("OnboardingPlanId", "JourneyPoint_OnboardingPlans").Nullable()
                .WithColumn("PlatformUserId").AsInt64().Nullable()
                .WithColumn("ManagerUserId").AsInt64().Nullable()
                .WithColumn("FullName").AsString(200).NotNullable()
                .WithColumn("EmailAddress").AsString(256).Nullable()
                .WithColumn("RoleTitle").AsString(200).Nullable()
                .WithColumn("Department").AsString(200).Nullable()
                .WithColumn("StartDate").AsDateTime().Nullable()
                .WithColumn("StatusLkp").AsInt64().Nullable()
                .WithColumn("WelcomeNotificationStatusLkp").AsInt64().Nullable()
                .WithColumn("WelcomeNotificationLastAttemptedAt").AsDateTime().Nullable()
                .WithColumn("WelcomeNotificationSentAt").AsDateTime().Nullable()
                .WithColumn("WelcomeNotificationFailureReason").AsString(500).Nullable()
                .WithColumn("ActivatedAt").AsDateTime().Nullable()
                .WithColumn("CompletedAt").AsDateTime().Nullable()
                .WithColumn("ExitedAt").AsDateTime().Nullable();
        }

        private void CreateJourneys()
        {
            Create.Table("JourneyPoint_Journeys")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithTenantIdAsRequired()
                .WithForeignKeyColumn("HireId", "JourneyPoint_Hires").Nullable()
                .WithForeignKeyColumn("OnboardingPlanId", "JourneyPoint_OnboardingPlans").Nullable()
                .WithColumn("StatusLkp").AsInt64().Nullable()
                .WithColumn("ActivatedAt").AsDateTime().Nullable()
                .WithColumn("PausedAt").AsDateTime().Nullable()
                .WithColumn("CompletedAt").AsDateTime().Nullable();
        }

        private void CreateJourneyTasks()
        {
            Create.Table("JourneyPoint_JourneyTasks")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithTenantIdAsRequired()
                .WithForeignKeyColumn("JourneyId", "JourneyPoint_Journeys").Nullable()
                .WithColumn("SourceOnboardingTaskId").AsGuid().Nullable()
                .WithColumn("SourceOnboardingModuleId").AsGuid().Nullable()
                .WithColumn("ModuleTitle").AsString(200).Nullable()
                .WithColumn("ModuleOrderIndex").AsInt32().WithDefaultValue(0)
                .WithColumn("TaskOrderIndex").AsInt32().WithDefaultValue(0)
                .WithColumn("Title").AsString(200).NotNullable()
                .WithColumn("Description").AsString(int.MaxValue).Nullable()
                .WithColumn("CategoryLkp").AsInt64().Nullable()
                .WithColumn("AssignmentTargetLkp").AsInt64().Nullable()
                .WithColumn("AcknowledgementRuleLkp").AsInt64().Nullable()
                .WithColumn("DueDayOffset").AsInt32().WithDefaultValue(0)
                .WithColumn("DueOn").AsDateTime().Nullable()
                .WithColumn("StatusLkp").AsInt64().Nullable()
                .WithColumn("AcknowledgedAt").AsDateTime().Nullable()
                .WithColumn("CompletedAt").AsDateTime().Nullable()
                .WithColumn("CompletedByUserId").AsInt64().Nullable()
                .WithColumn("PersonalisedAt").AsDateTime().Nullable();
        }

        private void CreateOnboardingDocuments()
        {
            Create.Table("JourneyPoint_OnboardingDocuments")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithTenantIdAsRequired()
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
        }

        private void CreateExtractedTasks()
        {
            Create.Table("JourneyPoint_ExtractedTasks")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithTenantIdAsRequired()
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

        private void CreateWellnessCheckIns()
        {
            Create.Table("JourneyPoint_WellnessCheckIns")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithTenantIdAsRequired()
                .WithForeignKeyColumn("HireId", "JourneyPoint_Hires").Nullable()
                .WithForeignKeyColumn("JourneyId", "JourneyPoint_Journeys").Nullable()
                .WithColumn("PeriodLkp").AsInt64().Nullable()
                .WithColumn("StatusLkp").AsInt64().Nullable()
                .WithColumn("ScheduledDate").AsDateTime().Nullable()
                .WithColumn("SubmittedAt").AsDateTime().Nullable()
                .WithColumn("InsightSummary").AsString(2000).Nullable();
        }

        private void CreateWellnessQuestions()
        {
            Create.Table("JourneyPoint_WellnessQuestions")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithTenantIdAsRequired()
                .WithForeignKeyColumn("WellnessCheckInId", "JourneyPoint_WellnessCheckIns").Nullable()
                .WithColumn("OrderIndex").AsInt32().WithDefaultValue(0)
                .WithColumn("QuestionText").AsString(1000).NotNullable()
                .WithColumn("AnswerText").AsString(int.MaxValue).Nullable()
                .WithColumn("AiSuggestedAnswer").AsString(int.MaxValue).Nullable()
                .WithColumn("IsAnswered").AsBoolean().WithDefaultValue(false);
        }

        private void CreateEngagementSnapshots()
        {
            Create.Table("JourneyPoint_EngagementSnapshots")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithTenantIdAsRequired()
                .WithForeignKeyColumn("HireId", "JourneyPoint_Hires").Nullable()
                .WithForeignKeyColumn("JourneyId", "JourneyPoint_Journeys").Nullable()
                .WithColumn("CompletionRate").AsDecimal(5, 4).WithDefaultValue(0)
                .WithColumn("DaysSinceLastActivity").AsInt32().WithDefaultValue(0)
                .WithColumn("OverdueTaskCount").AsInt32().WithDefaultValue(0)
                .WithColumn("CompositeScore").AsDecimal(5, 2).WithDefaultValue(0)
                .WithColumn("ClassificationLkp").AsInt64().Nullable()
                .WithColumn("ComputedAt").AsDateTime().NotNullable();

            Create.Index("IX_EngagementSnapshots_HireId_ComputedAt")
                .OnTable("JourneyPoint_EngagementSnapshots")
                .OnColumn("HireId").Ascending()
                .OnColumn("ComputedAt").Descending();
        }

        private void CreateAtRiskFlags()
        {
            Create.Table("JourneyPoint_AtRiskFlags")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithTenantIdAsRequired()
                .WithForeignKeyColumn("HireId", "JourneyPoint_Hires").Nullable()
                .WithForeignKeyColumn("JourneyId", "JourneyPoint_Journeys").Nullable()
                .WithColumn("RaisedAt").AsDateTime().NotNullable()
                .WithColumn("ClassificationAtRaiseLkp").AsInt64().Nullable()
                .WithColumn("StatusLkp").AsInt64().Nullable()
                .WithColumn("AcknowledgedByUserId").AsInt64().Nullable()
                .WithColumn("AcknowledgedAt").AsDateTime().Nullable()
                .WithColumn("AcknowledgementNotes").AsString(2000).Nullable()
                .WithColumn("ResolvedByUserId").AsInt64().Nullable()
                .WithColumn("ResolvedAt").AsDateTime().Nullable()
                .WithColumn("ResolutionTypeLkp").AsInt64().Nullable()
                .WithColumn("ResolutionNotes").AsString(2000).Nullable();
        }

        private void CreateGenerationLogs()
        {
            Create.Table("JourneyPoint_GenerationLogs")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithTenantIdAsRequired()
                .WithColumn("WorkflowTypeLkp").AsInt64().Nullable()
                .WithColumn("StatusLkp").AsInt64().Nullable()
                .WithColumn("HireId").AsGuid().Nullable()
                .WithColumn("JourneyId").AsGuid().Nullable()
                .WithColumn("OnboardingPlanId").AsGuid().Nullable()
                .WithColumn("OnboardingDocumentId").AsGuid().Nullable()
                .WithColumn("ModelName").AsString(200).NotNullable()
                .WithColumn("PromptSummary").AsString(int.MaxValue).NotNullable()
                .WithColumn("ResponseSummary").AsString(int.MaxValue).Nullable()
                .WithColumn("FailureReason").AsString(1000).Nullable()
                .WithColumn("TasksAdded").AsInt32().WithDefaultValue(0)
                .WithColumn("TasksRevised").AsInt32().WithDefaultValue(0)
                .WithColumn("StartedAt").AsDateTime().NotNullable()
                .WithColumn("CompletedAt").AsDateTime().NotNullable()
                .WithColumn("DurationMilliseconds").AsInt64().WithDefaultValue(0);
        }

        #endregion
    }
}
