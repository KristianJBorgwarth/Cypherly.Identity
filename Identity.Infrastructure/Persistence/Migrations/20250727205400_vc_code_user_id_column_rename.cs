using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class vc_code_user_id_column_rename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_verification_code_user_UserId",
                table: "user_verification_code");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "user_verification_code",
                newName: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_verification_code_user_user_id",
                table: "user_verification_code",
                column: "user_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_verification_code_user_user_id",
                table: "user_verification_code");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "user_verification_code",
                newName: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_user_verification_code_user_UserId",
                table: "user_verification_code",
                column: "UserId",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
