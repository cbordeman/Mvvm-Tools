// ReSharper disable InconsistentNaming

namespace MvvmTools.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Template_ForeignKeys2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MvvmTemplates", "ApplicationUserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.MvvmTemplates", new[] { "ApplicationUserId", "Name", "Language" }, unique: true, name: "UK_ApplicationUserId_Name_Language");
            CreateIndex("dbo.MvvmTemplates", "MvvmTemplateCategoryId");
            AddForeignKey("dbo.MvvmTemplates", "ApplicationUserId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.MvvmTemplates", "MvvmTemplateCategoryId", "dbo.MvvmTemplateCategories", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MvvmTemplates", "MvvmTemplateCategoryId", "dbo.MvvmTemplateCategories");
            DropForeignKey("dbo.MvvmTemplates", "ApplicationUserId", "dbo.AspNetUsers");
            DropIndex("dbo.MvvmTemplates", new[] { "MvvmTemplateCategoryId" });
            DropIndex("dbo.MvvmTemplates", "UK_ApplicationUserId_Name_Language");
            DropColumn("dbo.MvvmTemplates", "ApplicationUserId");
        }
    }
}
