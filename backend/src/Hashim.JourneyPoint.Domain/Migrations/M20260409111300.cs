using FluentMigrator;
using Shesha.FluentMigrator;

namespace Hashim.JourneyPoint.Domain.Migrations
{
    [Migration(20260409111300)]
    public class M20260409111300 : OneWayMigration
    {
        public override void Up()
        {
            // Undo: drop FK, index, and columns if they already exist (e.g. from auto-migration)
            Execute.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Persons_Frwk_StoredFiles_JourneyPoint_IdDocumentId')
                    ALTER TABLE [dbo].[Core_Persons] DROP CONSTRAINT [FK_Persons_Frwk_StoredFiles_JourneyPoint_IdDocumentId];

                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Persons_JourneyPoint_IdDocumentId' AND object_id = OBJECT_ID('[dbo].[Core_Persons]'))
                    DROP INDEX [IX_Persons_JourneyPoint_IdDocumentId] ON [dbo].[Core_Persons];

                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Core_Persons' AND COLUMN_NAME = 'JourneyPoint_MembershipNumber')
                    ALTER TABLE [dbo].[Core_Persons] DROP COLUMN [JourneyPoint_MembershipNumber];

                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Core_Persons' AND COLUMN_NAME = 'JourneyPoint_MembershipStartDate')
                    ALTER TABLE [dbo].[Core_Persons] DROP COLUMN [JourneyPoint_MembershipStartDate];

                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Core_Persons' AND COLUMN_NAME = 'JourneyPoint_MembershipEndDate')
                    ALTER TABLE [dbo].[Core_Persons] DROP COLUMN [JourneyPoint_MembershipEndDate];

                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Core_Persons' AND COLUMN_NAME = 'JourneyPoint_MembershipStatusLkp')
                    ALTER TABLE [dbo].[Core_Persons] DROP COLUMN [JourneyPoint_MembershipStatusLkp];

                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Core_Persons' AND COLUMN_NAME = 'JourneyPoint_IdDocumentId')
                    ALTER TABLE [dbo].[Core_Persons] DROP COLUMN [JourneyPoint_IdDocumentId];
            ");

            // Redo: add all columns, index, and FK cleanly
            Alter.Table("Core_Persons").InSchema("dbo")
                .AddColumn("JourneyPoint_MembershipNumber").AsString(500).Nullable()
                .AddColumn("JourneyPoint_MembershipStartDate").AsDateTime().Nullable()
                .AddColumn("JourneyPoint_MembershipEndDate").AsDateTime().Nullable()
                .AddColumn("JourneyPoint_MembershipStatusLkp").AsInt64().Nullable()
                .AddColumn("JourneyPoint_IdDocumentId").AsGuid().Nullable();

            Create.Index("IX_Persons_JourneyPoint_IdDocumentId")
                .OnTable("Core_Persons").InSchema("dbo")
                .OnColumn("JourneyPoint_IdDocumentId").Ascending();

            Create.ForeignKey("FK_Persons_Frwk_StoredFiles_JourneyPoint_IdDocumentId")
                .FromTable("Core_Persons").InSchema("dbo").ForeignColumn("JourneyPoint_IdDocumentId")
                .ToTable("stored_Files").InSchema("frwk").PrimaryColumn("id");
        }
    }
}