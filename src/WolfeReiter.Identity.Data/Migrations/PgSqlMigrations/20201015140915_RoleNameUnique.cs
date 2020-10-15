using Microsoft.EntityFrameworkCore.Migrations;

namespace WolfeReiter.Identity.Data.Migrations.PgSqlMigrations
{
    public partial class RoleNameUnique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_roles_name",
                table: "roles",
                column: "name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_roles_name",
                table: "roles");
        }
    }
}
