namespace SelfHost.Migrations.OperationalConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Consents",
                c => new
                    {
                        Subject = c.String(nullable: false, maxLength: 200),
                        ClientId = c.String(nullable: false, maxLength: 200),
                        Scopes = c.String(nullable: false, maxLength: 2000),
                    })
                .PrimaryKey(t => new { t.Subject, t.ClientId });
            
            CreateTable(
                "dbo.Tokens",
                c => new
                    {
                        Key = c.String(nullable: false, maxLength: 128),
                        TokenType = c.Short(nullable: false),
                        SubjectId = c.String(maxLength: 200),
                        ClientId = c.String(nullable: false, maxLength: 200),
                        JsonCode = c.String(nullable: false),
                        Expiry = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => new { t.Key, t.TokenType });
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Tokens");
            DropTable("dbo.Consents");
        }
    }
}
