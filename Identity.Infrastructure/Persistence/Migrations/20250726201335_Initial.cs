using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "outbox_message",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    occurred_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_message", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "citext", maxLength: 255, nullable: false),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "device",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    public_key = table.Column<string>(type: "text", nullable: false),
                    connection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    platform = table.Column<string>(type: "text", nullable: false),
                    app_version = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device", x => x.id);
                    table.ForeignKey(
                        name: "FK_device_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserVerificationCode",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false),
                    expiration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    code_type = table.Column<string>(type: "text", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVerificationCode", x => x.id);
                    table.ForeignKey(
                        name: "FK_UserVerificationCode_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    expires = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.id);
                    table.ForeignKey(
                        name: "FK_RefreshToken_device_device_id",
                        column: x => x.device_id,
                        principalTable: "device",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_device_user_id",
                table: "device",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "idx_token",
                table: "RefreshToken",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_device_id",
                table: "RefreshToken",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "idx_email",
                table: "user",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_expiration_date",
                table: "UserVerificationCode",
                column: "expiration_date");

            migrationBuilder.CreateIndex(
                name: "idx_user_id",
                table: "UserVerificationCode",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVerificationCode_code",
                table: "UserVerificationCode",
                column: "code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_message");

            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropTable(
                name: "UserVerificationCode");

            migrationBuilder.DropTable(
                name: "device");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}
