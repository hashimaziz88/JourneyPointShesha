using FluentMigrator;
using Shesha.FluentMigrator;

namespace Hashim.JourneyPoint.Domain.Migrations
{
    /// <summary>
    /// Creates EngagementSnapshot and AtRiskFlag tables.
    /// EngagementSnapshot is append-only — rows are never updated after insert.
    /// AtRiskFlag allows at most one Active flag per hire at any time.
    /// Depends on M20260409141000 (Hires and Journeys must exist).
    /// </summary>
    [Migration(20260409144000)]
    public class M20260409144000 : OneWayMigration
    {
        public override void Up()
        {
            // EngagementSnapshots — append-only
            Create.Table("JourneyPoint_EngagementSnapshots")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithColumn("TenantId").AsInt32().WithDefaultValue(1)
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

            // AtRiskFlags
            Create.Table("JourneyPoint_AtRiskFlags")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithColumn("TenantId").AsInt32().WithDefaultValue(1)
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
    }
}
