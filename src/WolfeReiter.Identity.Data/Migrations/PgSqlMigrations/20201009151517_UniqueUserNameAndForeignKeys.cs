using Microsoft.EntityFrameworkCore.Migrations;

namespace WolfeReiter.Identity.Data.Migrations.PgSqlMigrations
{
    public partial class UniqueUserNameAndForeignKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_users_name",
                table: "users",
                column: "name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_name",
                table: "users");
        }
    }
}
