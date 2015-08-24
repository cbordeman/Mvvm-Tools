// ReSharper disable InconsistentNaming

using System.Data.Entity.Migrations;

namespace MvvmTools.Web.Migrations
{
    public partial class Categories : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MvvmTemplateCategories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 60),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
            Sql("INSERT INTO dbo.MvvmTemplateCategories (Name) VALUES('Prism');");
            AddColumn("dbo.MvvmTemplates", "ApplicationUserId", c => c.Guid(nullable: false));
            AddColumn("dbo.MvvmTemplates", "MvvmTemplateCategoryId", c => c.Int(nullable: false));
            AlterColumn("dbo.MvvmTemplates", "Name", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.MvvmTemplates", "Language", c => c.String(nullable: false, maxLength: 15));
            Sql("UPDATE dbo.MvvmTemplates set [ViewModel] = '', [View] = '';");
            AlterColumn("dbo.MvvmTemplates", "ViewModel", c => c.String(nullable: false));
            AlterColumn("dbo.MvvmTemplates", "View", c => c.String(nullable: false));
            CreateIndex("dbo.MvvmTemplates", new[] { "ApplicationUserId", "Name", "Language" }, unique: true, name: "UK_ApplicationUserId_Name_Language");
            DropColumn("dbo.MvvmTemplates", "Category");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MvvmTemplates", "Category", c => c.String());
            DropIndex("dbo.MvvmTemplates", "UK_ApplicationUserId_Name_Language");
            DropIndex("dbo.MvvmTemplateCategories", new[] { "Name" });
            AlterColumn("dbo.MvvmTemplates", "View", c => c.String());
            AlterColumn("dbo.MvvmTemplates", "ViewModel", c => c.String());
            AlterColumn("dbo.MvvmTemplates", "Language", c => c.String());
            AlterColumn("dbo.MvvmTemplates", "Name", c => c.String());
            DropColumn("dbo.MvvmTemplates", "MvvmTemplateCategoryId");
            DropColumn("dbo.MvvmTemplates", "ApplicationUserId");
            DropTable("dbo.MvvmTemplateCategories");
        }
    }
}
