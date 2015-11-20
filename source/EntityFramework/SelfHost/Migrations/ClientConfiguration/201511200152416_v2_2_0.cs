namespace SelfHost.Migrations.ClientConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v2_2_0 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Clients", "LogoutUri", c => c.String());
            AddColumn("dbo.Clients", "LogoutSessionRequired", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Clients", "LogoutSessionRequired");
            DropColumn("dbo.Clients", "LogoutUri");
        }
    }
}
