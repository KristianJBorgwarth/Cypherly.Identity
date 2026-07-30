using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class add_signing_key : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "signing_key",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kid = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    n = table.Column<string>(type: "text", nullable: false),
                    e = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    kty = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    use = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    alg = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    private_key = table.Column<byte[]>(type: "bytea", nullable: false),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_signing_key", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_signing_key_expires_at",
                table: "signing_key",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "idx_signing_key_is_current",
                table: "signing_key",
                column: "is_current",
                unique: true,
                filter: "is_current");

            migrationBuilder.CreateIndex(
                name: "idx_signing_key_kid",
                table: "signing_key",
                column: "kid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "signing_key");
        }
    }
}
