namespace SelfHost.Migrations.ClientConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v2_5_0 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Clients", "AllowAccessTokensViaBrowser", c => c.Boolean(nullable: false, defaultValue:true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Clients", "AllowAccessTokensViaBrowser");
        }
    }
}
