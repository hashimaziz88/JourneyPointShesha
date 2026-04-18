using FluentMigrator;
using Shesha.FluentMigrator;

namespace Hashim.JourneyPoint.Domain.Migrations
{
    /// <summary>
    /// Creates GenerationLog table.
    /// Append-only audit table — one row per Groq API call, never updated after insert.
    /// API keys and full prompt payloads are never stored — only non-sensitive summaries.
    /// Depends on M20260409141000 and M20260409142000.
    /// </summary>
    [Migration(20260409145000)]
    public class M20260409145000 : OneWayMigration
    {
        public override void Up()
        {
            Create.Table("JourneyPoint_GenerationLogs")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithColumn("TenantId").AsInt32().WithDefaultValue(1)
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
    }
}
