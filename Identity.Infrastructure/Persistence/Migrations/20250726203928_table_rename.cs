using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class table_rename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshToken_device_device_id",
                table: "RefreshToken");

            migrationBuilder.DropForeignKey(
                name: "FK_UserVerificationCode_user_UserId",
                table: "UserVerificationCode");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserVerificationCode",
                table: "UserVerificationCode");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshToken",
                table: "RefreshToken");

            migrationBuilder.RenameTable(
                name: "UserVerificationCode",
                newName: "user_verification_code");

            migrationBuilder.RenameTable(
                name: "RefreshToken",
                newName: "refresh_token");

            migrationBuilder.RenameIndex(
                name: "IX_UserVerificationCode_code",
                table: "user_verification_code",
                newName: "IX_user_verification_code_code");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshToken_device_id",
                table: "refresh_token",
                newName: "IX_refresh_token_device_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_verification_code",
                table: "user_verification_code",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_refresh_token",
                table: "refresh_token",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_refresh_token_device_device_id",
                table: "refresh_token",
                column: "device_id",
                principalTable: "device",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_verification_code_user_UserId",
                table: "user_verification_code",
                column: "UserId",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_refresh_token_device_device_id",
                table: "refresh_token");

            migrationBuilder.DropForeignKey(
                name: "FK_user_verification_code_user_UserId",
                table: "user_verification_code");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_verification_code",
                table: "user_verification_code");

            migrationBuilder.DropPrimaryKey(
                name: "PK_refresh_token",
                table: "refresh_token");

            migrationBuilder.RenameTable(
                name: "user_verification_code",
                newName: "UserVerificationCode");

            migrationBuilder.RenameTable(
                name: "refresh_token",
                newName: "RefreshToken");

            migrationBuilder.RenameIndex(
                name: "IX_user_verification_code_code",
                table: "UserVerificationCode",
                newName: "IX_UserVerificationCode_code");

            migrationBuilder.RenameIndex(
                name: "IX_refresh_token_device_id",
                table: "RefreshToken",
                newName: "IX_RefreshToken_device_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserVerificationCode",
                table: "UserVerificationCode",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshToken",
                table: "RefreshToken",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshToken_device_device_id",
                table: "RefreshToken",
                column: "device_id",
                principalTable: "device",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserVerificationCode_user_UserId",
                table: "UserVerificationCode",
                column: "UserId",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
