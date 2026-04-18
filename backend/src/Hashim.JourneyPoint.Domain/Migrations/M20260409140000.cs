using FluentMigrator;
using Shesha.FluentMigrator;

namespace Hashim.JourneyPoint.Domain.Migrations
{
    /// <summary>
    /// Creates OnboardingPlan, OnboardingModule, and OnboardingTask tables.
    /// These have no JourneyPoint foreign key dependencies and must run before Hires migrations.
    /// </summary>
    [Migration(20260409140000)]
    public class M20260409140000 : OneWayMigration
    {
        public override void Up()
        {
            // OnboardingPlans
            Create.Table("JourneyPoint_OnboardingPlans")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithColumn("TenantId").AsInt32().WithDefaultValue(1)
                .WithColumn("Name").AsString(200).NotNullable()
                .WithColumn("Description").AsString(int.MaxValue).Nullable()
                .WithColumn("TargetAudience").AsString(200).Nullable()
                .WithColumn("DurationDays").AsInt32().WithDefaultValue(0)
                .WithColumn("StatusLkp").AsInt64().Nullable();

            // OnboardingModules
            Create.Table("JourneyPoint_OnboardingModules")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithColumn("TenantId").AsInt32().WithDefaultValue(1)
                .WithForeignKeyColumn("OnboardingPlanId", "JourneyPoint_OnboardingPlans").Nullable()
                .WithColumn("Name").AsString(200).NotNullable()
                .WithColumn("Description").AsString(int.MaxValue).Nullable()
                .WithColumn("OrderIndex").AsInt32().WithDefaultValue(0)
                .WithColumn("DurationDays").AsInt32().WithDefaultValue(0);

            // OnboardingTasks
            Create.Table("JourneyPoint_OnboardingTasks")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithColumn("TenantId").AsInt32().WithDefaultValue(1)
                .WithForeignKeyColumn("OnboardingModuleId", "JourneyPoint_OnboardingModules").Nullable()
                .WithColumn("Title").AsString(200).NotNullable()
                .WithColumn("Description").AsString(int.MaxValue).Nullable()
                .WithColumn("CategoryLkp").AsInt64().Nullable()
                .WithColumn("AssignmentTargetLkp").AsInt64().Nullable()
                .WithColumn("AcknowledgementRuleLkp").AsInt64().Nullable()
                .WithColumn("DueDayOffset").AsInt32().WithDefaultValue(0)
                .WithColumn("OrderIndex").AsInt32().WithDefaultValue(0);
        }
    }
}
