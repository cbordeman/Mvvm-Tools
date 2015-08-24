using System.Data.Entity.Migrations;
using MvvmTools.Shared;

// ReSharper disable InconsistentNaming

namespace MvvmTools.Web.Migrations
{
    public partial class Confirm_Admin_Email : DbMigration
    {
        public override void Up()
        {
            Sql($"UPDATE dbo.AspNetUsers SET EmailConfirmed=1 WHERE UserName='{Secrets.AdminUserName}'");
        }
        
        public override void Down()
        {
        }
    }
}
