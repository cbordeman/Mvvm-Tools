// ReSharper disable InconsistentNaming

namespace MvvmTools.Web.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class MvvmTemplate_Enabled : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE MvvmTemplates SET Enabled=1;");
        }
        
        public override void Down()
        {
        }
    }
}
