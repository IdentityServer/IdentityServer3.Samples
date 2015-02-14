namespace SelfHost.Migrations.ClientConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Clients",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Enabled = c.Boolean(nullable: false),
                        ClientId = c.String(nullable: false, maxLength: 200),
                        ClientName = c.String(nullable: false, maxLength: 200),
                        ClientUri = c.String(maxLength: 2000),
                        LogoUri = c.String(),
                        RequireConsent = c.Boolean(nullable: false),
                        AllowRememberConsent = c.Boolean(nullable: false),
                        Flow = c.Int(nullable: false),
                        IdentityTokenLifetime = c.Int(nullable: false),
                        AccessTokenLifetime = c.Int(nullable: false),
                        AuthorizationCodeLifetime = c.Int(nullable: false),
                        AbsoluteRefreshTokenLifetime = c.Int(nullable: false),
                        SlidingRefreshTokenLifetime = c.Int(nullable: false),
                        RefreshTokenUsage = c.Int(nullable: false),
                        RefreshTokenExpiration = c.Int(nullable: false),
                        AccessTokenType = c.Int(nullable: false),
                        EnableLocalLogin = c.Boolean(nullable: false),
                        IncludeJwtId = c.Boolean(nullable: false),
                        AlwaysSendClientClaims = c.Boolean(nullable: false),
                        PrefixClientClaims = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ClientClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Type = c.String(nullable: false, maxLength: 250),
                        Value = c.String(nullable: false, maxLength: 250),
                        Client_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.Client_Id, cascadeDelete: true)
                .Index(t => t.Client_Id);
            
            CreateTable(
                "dbo.ClientSecrets",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Value = c.String(nullable: false, maxLength: 250),
                        ClientSecretType = c.String(maxLength: 250),
                        Description = c.String(maxLength: 2000),
                        Expiration = c.DateTimeOffset(precision: 7),
                        Client_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.Client_Id, cascadeDelete: true)
                .Index(t => t.Client_Id);
            
            CreateTable(
                "dbo.ClientGrantTypeRestrictions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GrantType = c.String(nullable: false, maxLength: 250),
                        Client_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.Client_Id, cascadeDelete: true)
                .Index(t => t.Client_Id);
            
            CreateTable(
                "dbo.ClientIdPRestrictions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Provider = c.String(nullable: false, maxLength: 200),
                        Client_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.Client_Id, cascadeDelete: true)
                .Index(t => t.Client_Id);
            
            CreateTable(
                "dbo.ClientPostLogoutRedirectUris",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Uri = c.String(nullable: false, maxLength: 2000),
                        Client_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.Client_Id, cascadeDelete: true)
                .Index(t => t.Client_Id);
            
            CreateTable(
                "dbo.ClientRedirectUris",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Uri = c.String(nullable: false, maxLength: 2000),
                        Client_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.Client_Id, cascadeDelete: true)
                .Index(t => t.Client_Id);
            
            CreateTable(
                "dbo.ClientScopeRestrictions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Scope = c.String(nullable: false, maxLength: 200),
                        Client_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.Client_Id, cascadeDelete: true)
                .Index(t => t.Client_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ClientScopeRestrictions", "Client_Id", "dbo.Clients");
            DropForeignKey("dbo.ClientRedirectUris", "Client_Id", "dbo.Clients");
            DropForeignKey("dbo.ClientPostLogoutRedirectUris", "Client_Id", "dbo.Clients");
            DropForeignKey("dbo.ClientIdPRestrictions", "Client_Id", "dbo.Clients");
            DropForeignKey("dbo.ClientGrantTypeRestrictions", "Client_Id", "dbo.Clients");
            DropForeignKey("dbo.ClientSecrets", "Client_Id", "dbo.Clients");
            DropForeignKey("dbo.ClientClaims", "Client_Id", "dbo.Clients");
            DropIndex("dbo.ClientScopeRestrictions", new[] { "Client_Id" });
            DropIndex("dbo.ClientRedirectUris", new[] { "Client_Id" });
            DropIndex("dbo.ClientPostLogoutRedirectUris", new[] { "Client_Id" });
            DropIndex("dbo.ClientIdPRestrictions", new[] { "Client_Id" });
            DropIndex("dbo.ClientGrantTypeRestrictions", new[] { "Client_Id" });
            DropIndex("dbo.ClientSecrets", new[] { "Client_Id" });
            DropIndex("dbo.ClientClaims", new[] { "Client_Id" });
            DropTable("dbo.ClientScopeRestrictions");
            DropTable("dbo.ClientRedirectUris");
            DropTable("dbo.ClientPostLogoutRedirectUris");
            DropTable("dbo.ClientIdPRestrictions");
            DropTable("dbo.ClientGrantTypeRestrictions");
            DropTable("dbo.ClientSecrets");
            DropTable("dbo.ClientClaims");
            DropTable("dbo.Clients");
        }
    }
}
