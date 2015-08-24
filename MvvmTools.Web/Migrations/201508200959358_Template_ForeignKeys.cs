// ReSharper disable InconsistentNaming

using System.Data.Entity.Migrations;

namespace MvvmTools.Web.Migrations
{
    public partial class Template_ForeignKeys : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.MvvmTemplates", "UK_ApplicationUserId_Name_Language");
            DropColumn("dbo.MvvmTemplates", "ApplicationUserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MvvmTemplates", "ApplicationUserId", c => c.Guid(nullable: false));
            CreateIndex("dbo.MvvmTemplates", new[] { "ApplicationUserId", "Name", "Language" }, unique: true, name: "UK_ApplicationUserId_Name_Language");
        }
    }
}
