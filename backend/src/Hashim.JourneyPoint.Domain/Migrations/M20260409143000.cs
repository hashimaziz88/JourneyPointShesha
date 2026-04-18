using FluentMigrator;
using Shesha.FluentMigrator;

namespace Hashim.JourneyPoint.Domain.Migrations
{
    /// <summary>
    /// Creates WellnessCheckIn and WellnessQuestion tables.
    /// Check-ins are scheduled at milestone periods (Day30, Day60, Day90, Day120).
    /// Depends on M20260409141000 (Hires and Journeys must exist).
    /// </summary>
    [Migration(20260409143000)]
    public class M20260409143000 : OneWayMigration
    {
        public override void Up()
        {
            // WellnessCheckIns
            Create.Table("JourneyPoint_WellnessCheckIns")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithColumn("TenantId").AsInt32().WithDefaultValue(1)
                .WithForeignKeyColumn("HireId", "JourneyPoint_Hires").Nullable()
                .WithForeignKeyColumn("JourneyId", "JourneyPoint_Journeys").Nullable()
                .WithColumn("PeriodLkp").AsInt64().Nullable()
                .WithColumn("StatusLkp").AsInt64().Nullable()
                .WithColumn("ScheduledDate").AsDateTime().Nullable()
                .WithColumn("SubmittedAt").AsDateTime().Nullable()
                .WithColumn("InsightSummary").AsString(2000).Nullable();

            // WellnessQuestions
            Create.Table("JourneyPoint_WellnessQuestions")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithColumn("TenantId").AsInt32().WithDefaultValue(1)
                .WithForeignKeyColumn("WellnessCheckInId", "JourneyPoint_WellnessCheckIns").Nullable()
                .WithColumn("OrderIndex").AsInt32().WithDefaultValue(0)
                .WithColumn("QuestionText").AsString(1000).NotNullable()
                .WithColumn("AnswerText").AsString(int.MaxValue).Nullable()
                .WithColumn("AiSuggestedAnswer").AsString(int.MaxValue).Nullable()
                .WithColumn("IsAnswered").AsBoolean().WithDefaultValue(false);
        }
    }
}
