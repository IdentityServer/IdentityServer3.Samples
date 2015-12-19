namespace SelfHost.Migrations.ScopeConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v2_3_0 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Scopes", "AllowUnrestrictedIntrospection", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Scopes", "AllowUnrestrictedIntrospection");
        }
    }
}
