using FluentMigrator;
using Shesha.FluentMigrator;

namespace Hashim.JourneyPoint.Domain.Migrations
{
    /// <summary>
    /// Removes all Member and MembershipPayment artefacts from the database.
    /// Drops the JourneyPoint_MembershipPayments table and all JourneyPoint_* columns
    /// that were added to Core_Persons by M20260409111300.
    /// All statements are guarded with IF EXISTS for idempotency.
    /// </summary>
    [Migration(20260413110000)]
    public class M20260413110000 : OneWayMigration
    {
        public override void Up()
        {
            // Drop MembershipPayments table (FK to Core_Persons first)
            Execute.Sql(@"
                IF OBJECT_ID('dbo.JourneyPoint_MembershipPayments', 'U') IS NOT NULL
                    DROP TABLE [dbo].[JourneyPoint_MembershipPayments];
            ");

            // Drop FK on IdDocumentId before dropping the column
            Execute.Sql(@"
                DECLARE @fk NVARCHAR(256);
                SELECT @fk = name
                FROM   sys.foreign_keys
                WHERE  parent_object_id = OBJECT_ID('dbo.Core_Persons')
                  AND  name LIKE '%IdDocument%';
                IF @fk IS NOT NULL
                    EXEC('ALTER TABLE [dbo].[Core_Persons] DROP CONSTRAINT [' + @fk + ']');
            ");

            // Drop each Member column from Core_Persons
            Execute.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                           WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Core_Persons'
                             AND COLUMN_NAME = 'JourneyPoint_MembershipNumber')
                    ALTER TABLE [dbo].[Core_Persons] DROP COLUMN [JourneyPoint_MembershipNumber];
            ");

            Execute.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                           WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Core_Persons'
                             AND COLUMN_NAME = 'JourneyPoint_MembershipStartDate')
                    ALTER TABLE [dbo].[Core_Persons] DROP COLUMN [JourneyPoint_MembershipStartDate];
            ");

            Execute.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                           WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Core_Persons'
                             AND COLUMN_NAME = 'JourneyPoint_MembershipEndDate')
                    ALTER TABLE [dbo].[Core_Persons] DROP COLUMN [JourneyPoint_MembershipEndDate];
            ");

            Execute.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                           WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Core_Persons'
                             AND COLUMN_NAME = 'JourneyPoint_MembershipStatusLkp')
                    ALTER TABLE [dbo].[Core_Persons] DROP COLUMN [JourneyPoint_MembershipStatusLkp];
            ");

            Execute.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes
                           WHERE object_id = OBJECT_ID('dbo.Core_Persons')
                             AND name = 'IX_Persons_JourneyPoint_IdDocumentId')
                    DROP INDEX [IX_Persons_JourneyPoint_IdDocumentId] ON [dbo].[Core_Persons];
            ");

            Execute.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                           WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Core_Persons'
                             AND COLUMN_NAME = 'JourneyPoint_IdDocumentId')
                    ALTER TABLE [dbo].[Core_Persons] DROP COLUMN [JourneyPoint_IdDocumentId];
            ");
        }
    }
}
