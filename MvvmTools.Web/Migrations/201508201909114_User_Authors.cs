namespace MvvmTools.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class User_Authors : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "ShowTemplates", c => c.Boolean(nullable: false));
            Sql("UPDATE dbo.AspNetUsers SET ShowTemplates = 1;");
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "ShowTemplates");
        }
    }
}
