// ReSharper disable InconsistentNaming

using System.Data.Entity.Migrations;

namespace MvvmTools.Web.Migrations
{
    public partial class Author : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MvvmTemplates", "Enabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.AspNetUsers", "Author", c => c.String(nullable: false, maxLength: 100));
            CreateIndex("dbo.AspNetUsers", "Author", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.AspNetUsers", new[] { "Author" });
            DropColumn("dbo.AspNetUsers", "Author");
            DropColumn("dbo.MvvmTemplates", "Enabled");
        }
    }
}
