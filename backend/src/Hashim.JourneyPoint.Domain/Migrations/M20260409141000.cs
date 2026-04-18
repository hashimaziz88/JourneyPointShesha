using FluentMigrator;
using Shesha.FluentMigrator;

namespace Hashim.JourneyPoint.Domain.Migrations
{
    /// <summary>
    /// Creates Hire, Journey, and JourneyTask tables.
    /// Depends on M20260409140000 (OnboardingPlans must exist before Hires can reference them).
    /// </summary>
    [Migration(20260409141000)]
    public class M20260409141000 : OneWayMigration
    {
        public override void Up()
        {
            // Hires
            Create.Table("JourneyPoint_Hires")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithColumn("TenantId").AsInt32().WithDefaultValue(1)
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

            // Journeys
            Create.Table("JourneyPoint_Journeys")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithColumn("TenantId").AsInt32().WithDefaultValue(1)
                .WithForeignKeyColumn("HireId", "JourneyPoint_Hires").Nullable()
                .WithForeignKeyColumn("OnboardingPlanId", "JourneyPoint_OnboardingPlans").Nullable()
                .WithColumn("StatusLkp").AsInt64().Nullable()
                .WithColumn("ActivatedAt").AsDateTime().Nullable()
                .WithColumn("PausedAt").AsDateTime().Nullable()
                .WithColumn("CompletedAt").AsDateTime().Nullable();

            // JourneyTasks
            Create.Table("JourneyPoint_JourneyTasks")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithColumn("TenantId").AsInt32().WithDefaultValue(1)
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
    }
}
