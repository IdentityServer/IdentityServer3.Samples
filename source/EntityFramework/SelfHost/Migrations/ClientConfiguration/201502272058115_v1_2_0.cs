namespace SelfHost.Migrations.ClientConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v1_2_0 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClientCorsOrigins",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Origin = c.String(nullable: false, maxLength: 150),
                        Client_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.Client_Id, cascadeDelete: true)
                .Index(t => t.Client_Id);
            
            AddColumn("dbo.Clients", "AllowClientCredentialsOnly", c => c.Boolean(nullable: false));
            AddColumn("dbo.Clients", "UpdateAccessTokenClaimsOnRefresh", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ClientCorsOrigins", "Client_Id", "dbo.Clients");
            DropIndex("dbo.ClientCorsOrigins", new[] { "Client_Id" });
            DropColumn("dbo.Clients", "UpdateAccessTokenClaimsOnRefresh");
            DropColumn("dbo.Clients", "AllowClientCredentialsOnly");
            DropTable("dbo.ClientCorsOrigins");
        }
    }
}
