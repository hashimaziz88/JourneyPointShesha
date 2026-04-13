using FluentMigrator;
using Shesha.FluentMigrator;

namespace Hashim.JourneyPoint.Domain.Migrations
{
    /// <summary>
    /// Adds JourneyId column to JourneyPoint_Hires.
    /// NHibernate auto-generates a JourneyId FK column on the Hires table for the
    /// Hire.Journey navigation property. Without this column, every INSERT into
    /// JourneyPoint_Hires throws "Invalid column name 'JourneyId'".
    /// Guarded with IF NOT EXISTS for idempotency.
    /// </summary>
    [Migration(20260413150000)]
    public class M20260413150000 : OneWayMigration
    {
        public override void Up()
        {
            Execute.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = 'dbo'
                      AND TABLE_NAME   = 'JourneyPoint_Hires'
                      AND COLUMN_NAME  = 'JourneyId')
                ALTER TABLE [dbo].[JourneyPoint_Hires]
                    ADD [JourneyId] UNIQUEIDENTIFIER NULL;
            ");
        }
    }
}
