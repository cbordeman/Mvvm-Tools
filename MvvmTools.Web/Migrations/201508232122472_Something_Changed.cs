namespace MvvmTools.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Something_Changed : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE dbo.MvvmTemplates SET dbo.MvvmTemplates.Tags = 'Bacon Ipsum' WHERE ISNULL(dbo.MvvmTemplates.Tags, NULL) = NULL");
            AlterColumn("dbo.MvvmTemplates", "Tags", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.MvvmTemplates", "Tags", c => c.String());
        }
    }
}
