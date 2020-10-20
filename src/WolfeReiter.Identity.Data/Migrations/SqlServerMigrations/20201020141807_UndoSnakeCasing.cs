using Microsoft.EntityFrameworkCore.Migrations;

namespace WolfeReiter.Identity.Data.Migrations.SqlServerMigrations
{
    public partial class UndoSnakeCasing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_roles_role_id",
                table: "user_roles");

            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_users_user_id",
                table: "user_roles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_roles",
                table: "roles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_roles",
                table: "user_roles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_data_transforms_history",
                table: "data_transforms_history");

            migrationBuilder.DropPrimaryKey(
                name: "pk_data_protection_keys",
                table: "data_protection_keys");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "roles",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "user_roles",
                newName: "UserRoles");

            migrationBuilder.RenameTable(
                name: "data_transforms_history",
                newName: "DataTransformsHistory");

            migrationBuilder.RenameTable(
                name: "data_protection_keys",
                newName: "DataProtectionKeys");

            migrationBuilder.RenameColumn(
                name: "surname",
                table: "Users",
                newName: "Surname");

            migrationBuilder.RenameColumn(
                name: "salt",
                table: "Users",
                newName: "Salt");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Users",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "locked",
                table: "Users",
                newName: "Locked");

            migrationBuilder.RenameColumn(
                name: "hash",
                table: "Users",
                newName: "Hash");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "created",
                table: "Users",
                newName: "Created");

            migrationBuilder.RenameColumn(
                name: "active",
                table: "Users",
                newName: "Active");

            migrationBuilder.RenameColumn(
                name: "user_number",
                table: "Users",
                newName: "UserNumber");

            migrationBuilder.RenameColumn(
                name: "last_login_attempt",
                table: "Users",
                newName: "LastLoginAttempt");

            migrationBuilder.RenameColumn(
                name: "given_name",
                table: "Users",
                newName: "GivenName");

            migrationBuilder.RenameColumn(
                name: "failed_login_attempts",
                table: "Users",
                newName: "FailedLoginAttempts");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Users",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "ix_users_name",
                table: "Users",
                newName: "IX_Users_Name");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Roles",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "Roles",
                newName: "RoleId");

            migrationBuilder.RenameIndex(
                name: "ix_roles_name",
                table: "Roles",
                newName: "IX_Roles_Name");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "UserRoles",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "UserRoles",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "user_role_id",
                table: "UserRoles",
                newName: "UserRoleId");

            migrationBuilder.RenameIndex(
                name: "ix_user_roles_user_id",
                table: "UserRoles",
                newName: "IX_UserRoles_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_user_roles_role_id",
                table: "UserRoles",
                newName: "IX_UserRoles_RoleId");

            migrationBuilder.RenameColumn(
                name: "applied",
                table: "DataTransformsHistory",
                newName: "Applied");

            migrationBuilder.RenameColumn(
                name: "transform_id",
                table: "DataTransformsHistory",
                newName: "TransformId");

            migrationBuilder.RenameColumn(
                name: "xml",
                table: "DataProtectionKeys",
                newName: "Xml");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "DataProtectionKeys",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "friendly_name",
                table: "DataProtectionKeys",
                newName: "FriendlyName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "RoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                column: "UserRoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DataTransformsHistory",
                table: "DataTransformsHistory",
                column: "TransformId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DataProtectionKeys",
                table: "DataProtectionKeys",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DataTransformsHistory",
                table: "DataTransformsHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DataProtectionKeys",
                table: "DataProtectionKeys");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "roles");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                newName: "user_roles");

            migrationBuilder.RenameTable(
                name: "DataTransformsHistory",
                newName: "data_transforms_history");

            migrationBuilder.RenameTable(
                name: "DataProtectionKeys",
                newName: "data_protection_keys");

            migrationBuilder.RenameColumn(
                name: "Surname",
                table: "users",
                newName: "surname");

            migrationBuilder.RenameColumn(
                name: "Salt",
                table: "users",
                newName: "salt");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "users",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Locked",
                table: "users",
                newName: "locked");

            migrationBuilder.RenameColumn(
                name: "Hash",
                table: "users",
                newName: "hash");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "users",
                newName: "created");

            migrationBuilder.RenameColumn(
                name: "Active",
                table: "users",
                newName: "active");

            migrationBuilder.RenameColumn(
                name: "UserNumber",
                table: "users",
                newName: "user_number");

            migrationBuilder.RenameColumn(
                name: "LastLoginAttempt",
                table: "users",
                newName: "last_login_attempt");

            migrationBuilder.RenameColumn(
                name: "GivenName",
                table: "users",
                newName: "given_name");

            migrationBuilder.RenameColumn(
                name: "FailedLoginAttempts",
                table: "users",
                newName: "failed_login_attempts");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "users",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Name",
                table: "users",
                newName: "ix_users_name");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "roles",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "roles",
                newName: "role_id");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_Name",
                table: "roles",
                newName: "ix_roles_name");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "user_roles",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "user_roles",
                newName: "role_id");

            migrationBuilder.RenameColumn(
                name: "UserRoleId",
                table: "user_roles",
                newName: "user_role_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserRoles_UserId",
                table: "user_roles",
                newName: "ix_user_roles_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserRoles_RoleId",
                table: "user_roles",
                newName: "ix_user_roles_role_id");

            migrationBuilder.RenameColumn(
                name: "Applied",
                table: "data_transforms_history",
                newName: "applied");

            migrationBuilder.RenameColumn(
                name: "TransformId",
                table: "data_transforms_history",
                newName: "transform_id");

            migrationBuilder.RenameColumn(
                name: "Xml",
                table: "data_protection_keys",
                newName: "xml");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "data_protection_keys",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "FriendlyName",
                table: "data_protection_keys",
                newName: "friendly_name");

            migrationBuilder.AddPrimaryKey(
                name: "pk_users",
                table: "users",
                column: "user_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_roles",
                table: "roles",
                column: "role_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_roles",
                table: "user_roles",
                column: "user_role_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_data_transforms_history",
                table: "data_transforms_history",
                column: "transform_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_data_protection_keys",
                table: "data_protection_keys",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_roles_role_id",
                table: "user_roles",
                column: "role_id",
                principalTable: "roles",
                principalColumn: "role_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_users_user_id",
                table: "user_roles",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
