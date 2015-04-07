namespace SelfHost.Migrations.ClientConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v2_0_0 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ClientGrantTypeRestrictions", newName: "ClientCustomGrantTypes");
            RenameTable(name: "dbo.ClientScopeRestrictions", newName: "ClientScopes");
            AddColumn("dbo.Clients", "AllowAccessToAllScopes", c => c.Boolean(nullable: false));
            AddColumn("dbo.Clients", "AllowAccessToAllCustomGrantTypes", c => c.Boolean(nullable: false));
            RenameColumn("dbo.ClientSecrets", "ClientSecretType", "Type");
            CreateIndex("dbo.Clients", "ClientId", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.Clients", new[] { "ClientId" });
            RenameColumn("dbo.ClientSecrets", "Type", "ClientSecretType");
            DropColumn("dbo.Clients", "AllowAccessToAllCustomGrantTypes");
            DropColumn("dbo.Clients", "AllowAccessToAllScopes");
            RenameTable(name: "dbo.ClientScopes", newName: "ClientScopeRestrictions");
            RenameTable(name: "dbo.ClientCustomGrantTypes", newName: "ClientGrantTypeRestrictions");
        }
    }
}
