using FluentMigrator;
using Shesha.FluentMigrator;

namespace Hashim.JourneyPoint.Domain.Migrations
{
    /// <summary>
    /// Re-seeds the Enrolee, Facilitator, and Manager static roles.
    /// M20260413120000 ran before Shesha created the default tenant, so AbpTenants
    /// was empty and no rows were inserted. This migration runs after the tenant
    /// exists and catches up.
    /// Also seeds host-level roles (TenantId IS NULL) in case the admin user
    /// operates in host context.
    /// All statements are idempotent via NOT EXISTS guards.
    /// </summary>
    [Migration(20260413130000)]
    public class M20260413130000 : OneWayMigration
    {
        public override void Up()
        {
            // Seed for every tenant that currently exists
            Execute.Sql(@"
                INSERT INTO [dbo].[AbpRoles]
                    ([TenantId], [Name], [DisplayName], [IsStatic], [IsDefault],
                     [NormalizedName], [IsDeleted], [CreationTime], [ConcurrencyStamp])
                SELECT
                    t.[Id]          AS TenantId,
                    r.[Name]        AS Name,
                    r.[Name]        AS DisplayName,
                    1               AS IsStatic,
                    0               AS IsDefault,
                    UPPER(r.[Name]) AS NormalizedName,
                    0               AS IsDeleted,
                    GETUTCDATE()    AS CreationTime,
                    NEWID()         AS ConcurrencyStamp
                FROM [dbo].[AbpTenants] t
                CROSS JOIN (
                    VALUES ('Enrolee'), ('Facilitator'), ('Manager')
                ) r ([Name])
                WHERE t.[IsDeleted] = 0
                  AND NOT EXISTS (
                      SELECT 1 FROM [dbo].[AbpRoles] ar
                      WHERE ar.[TenantId] = t.[Id]
                        AND ar.[Name]     = r.[Name]
                        AND ar.[IsDeleted] = 0
                  );
            ");

            // Seed for host level (TenantId IS NULL)
            Execute.Sql(@"
                INSERT INTO [dbo].[AbpRoles]
                    ([TenantId], [Name], [DisplayName], [IsStatic], [IsDefault],
                     [NormalizedName], [IsDeleted], [CreationTime], [ConcurrencyStamp])
                SELECT
                    NULL,
                    r.[Name],
                    r.[Name],
                    1,
                    0,
                    UPPER(r.[Name]),
                    0,
                    GETUTCDATE(),
                    NEWID()
                FROM (VALUES ('Enrolee'), ('Facilitator'), ('Manager')) r ([Name])
                WHERE NOT EXISTS (
                    SELECT 1 FROM [dbo].[AbpRoles] ar
                    WHERE ar.[TenantId] IS NULL
                      AND ar.[Name]     = r.[Name]
                      AND ar.[IsDeleted] = 0
                );
            ");
        }
    }
}
