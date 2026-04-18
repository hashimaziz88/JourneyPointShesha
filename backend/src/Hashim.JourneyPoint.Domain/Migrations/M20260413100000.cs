using FluentMigrator;
using Shesha.FluentMigrator;

namespace Hashim.JourneyPoint.Domain.Migrations
{
    /// <summary>
    /// Adds the TenantId column to JourneyPoint_OnboardingPlans.
    /// M20260410100000 intentionally deferred multi-tenancy; this migration implements it.
    /// The column is added with a default of 0 (host tenant) so existing rows remain valid.
    /// </summary>
    [Migration(20260413100000)]
    public class M20260413100000 : OneWayMigration
    {
        public override void Up()
        {
            Execute.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = 'dbo'
                      AND TABLE_NAME   = 'JourneyPoint_OnboardingPlans'
                      AND COLUMN_NAME  = 'TenantId'
                )
                BEGIN
                    ALTER TABLE [dbo].[JourneyPoint_OnboardingPlans]
                        ADD [TenantId] INT NOT NULL DEFAULT 0;
                END
            ");
        }
    }
}
