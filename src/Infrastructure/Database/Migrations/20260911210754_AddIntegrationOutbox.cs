using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nesto.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddIntegrationOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "integration_outbox_messages",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    content = table.Column<string>(type: "jsonb", nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    attempts = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_integration_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_integration_outbox_messages_processed_on_utc_occurred_on_utc",
                schema: "public",
                table: "integration_outbox_messages",
                columns: new[] { "processed_on_utc", "occurred_on_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "integration_outbox_messages",
                schema: "public");
        }
    }
}
