using FluentMigrator;
using Microsoft.AspNetCore.Http.HttpResults;
using Shesha.FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hashim.JourneyPoint.Domain.Migrations
{
    
    [Migration(20260409131000)]
    public class M20260409131000 : Migration
    {
        /// <summary>
        /// Code to execute when executing the migrations
        /// </summary>
        public override void Up()
        {
            Create.Table("JourneyPoint_MembershipPayments")
                .WithIdAsGuid()
                .WithFullAuditColumns()
                .WithForeignKeyColumn("MemberId", "Core_Persons").Nullable()
                .WithColumn("Amount").AsDouble().Nullable()
                .WithColumn("PaymentDate").AsDateTime().Nullable();
        }
        /// <summary>
        /// Code to execute when rolling back the migration
        /// </summary>
        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
