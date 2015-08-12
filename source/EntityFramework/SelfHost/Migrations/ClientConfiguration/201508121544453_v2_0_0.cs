namespace SelfHost.Migrations.ClientConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v2_0_0 : DbMigration
    {
        // Comments in the Up() and Down() method were added manually
        // the EF migrations didn't know that columns were just being renamed, so
        // the migration was manually changed to use RenameColumn to preserve data
        public override void Up()
        {
            RenameTable(name: "dbo.ClientGrantTypeRestrictions", newName: "ClientCustomGrantTypes");
            RenameTable(name: "dbo.ClientScopeRestrictions", newName: "ClientScopes");
            AddColumn("dbo.Clients", "AllowAccessToAllScopes", c => c.Boolean(nullable: false));
            //AddColumn("dbo.Clients", "UpdateAccessTokenOnRefresh", c => c.Boolean(nullable: false));
            RenameColumn("dbo.Clients", "UpdateAccessTokenClaimsOnRefresh", "UpdateAccessTokenOnRefresh");
            AddColumn("dbo.Clients", "AllowAccessToAllGrantTypes", c => c.Boolean(nullable: false));
            //AddColumn("dbo.ClientSecrets", "Type", c => c.String(maxLength: 250));
            RenameColumn("dbo.ClientSecrets", "ClientSecretType", "Type");
            CreateIndex("dbo.Clients", "ClientId", unique: true);
            //DropColumn("dbo.Clients", "UpdateAccessTokenClaimsOnRefresh");
            //DropColumn("dbo.ClientSecrets", "ClientSecretType");
        }
        
        public override void Down()
        {
            //AddColumn("dbo.ClientSecrets", "ClientSecretType", c => c.String(maxLength: 250));
            //AddColumn("dbo.Clients", "UpdateAccessTokenClaimsOnRefresh", c => c.Boolean(nullable: false));
            RenameColumn("dbo.Clients", "UpdateAccessTokenOnRefresh", "UpdateAccessTokenClaimsOnRefresh");
            DropIndex("dbo.Clients", new[] { "ClientId" });
            //DropColumn("dbo.ClientSecrets", "Type");
            DropColumn("dbo.Clients", "AllowAccessToAllGrantTypes");
            //DropColumn("dbo.Clients", "UpdateAccessTokenOnRefresh");
            DropColumn("dbo.Clients", "AllowAccessToAllScopes");
            RenameTable(name: "dbo.ClientScopes", newName: "ClientScopeRestrictions");
            RenameTable(name: "dbo.ClientCustomGrantTypes", newName: "ClientGrantTypeRestrictions");
        }
    }
}
