using FluentMigrator;
using Shesha.FluentMigrator;

namespace Hashim.JourneyPoint.Domain.Migrations
{
    /// <summary>
    /// Ensures JourneyId column exists on JourneyPoint_JourneyTasks and
    /// JourneyPoint_WellnessCheckIns. Earlier migrations may not have applied
    /// the column correctly if FK constraints blocked the DropIfExists calls.
    /// All statements are guarded with IF NOT EXISTS for idempotency.
    /// </summary>
    [Migration(20260413140000)]
    public class M20260413140000 : OneWayMigration
    {
        public override void Up()
        {
            Execute.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = 'dbo'
                      AND TABLE_NAME   = 'JourneyPoint_JourneyTasks'
                      AND COLUMN_NAME  = 'JourneyId')
                ALTER TABLE [dbo].[JourneyPoint_JourneyTasks]
                    ADD [JourneyId] UNIQUEIDENTIFIER NULL;
            ");

            Execute.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = 'dbo'
                      AND TABLE_NAME   = 'JourneyPoint_WellnessCheckIns'
                      AND COLUMN_NAME  = 'JourneyId')
                ALTER TABLE [dbo].[JourneyPoint_WellnessCheckIns]
                    ADD [JourneyId] UNIQUEIDENTIFIER NULL;
            ");

            Execute.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = 'dbo'
                      AND TABLE_NAME   = 'JourneyPoint_EngagementSnapshots'
                      AND COLUMN_NAME  = 'JourneyId')
                ALTER TABLE [dbo].[JourneyPoint_EngagementSnapshots]
                    ADD [JourneyId] UNIQUEIDENTIFIER NULL;
            ");

            Execute.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = 'dbo'
                      AND TABLE_NAME   = 'JourneyPoint_AtRiskFlags'
                      AND COLUMN_NAME  = 'JourneyId')
                ALTER TABLE [dbo].[JourneyPoint_AtRiskFlags]
                    ADD [JourneyId] UNIQUEIDENTIFIER NULL;
            ");
        }
    }
}
