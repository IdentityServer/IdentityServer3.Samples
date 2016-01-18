namespace SelfHost.Migrations.ClientConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v2_4_0 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Clients", "RequireSignOutPrompt", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Clients", "RequireSignOutPrompt");
        }
    }
}
