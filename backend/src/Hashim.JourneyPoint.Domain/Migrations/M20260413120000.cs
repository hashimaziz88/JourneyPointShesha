using FluentMigrator;
using Shesha.FluentMigrator;

namespace Hashim.JourneyPoint.Domain.Migrations
{
    /// <summary>
    /// Seeds the Enrolee, Facilitator, and Manager static roles into AbpRoles
    /// for every tenant that currently exists in AbpTenants.
    /// Each INSERT is guarded with NOT EXISTS so re-runs are safe.
    /// New tenants created after this migration receive roles automatically
    /// via ABP's static role propagation on tenant creation.
    /// </summary>
    [Migration(20260413120000)]
    public class M20260413120000 : OneWayMigration
    {
        public override void Up()
        {
            // Seed Enrolee, Facilitator, Manager for every existing tenant.
            // The CROSS JOIN produces one row per (tenant × role) pair.
            // NOT EXISTS guard makes it idempotent.
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
        }
    }
}
