using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WolfeReiter.Identity.Data.Migrations.PgSqlMigrations
{
    public partial class DataTransformsHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "data_transforms_history",
                columns: table => new
                {
                    transform_id = table.Column<string>(maxLength: 150, nullable: false),
                    applied = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_transforms_history", x => x.transform_id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_transforms_history");
        }
    }
}
