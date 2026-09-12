using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nesto.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainEventIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "processed_domain_event_handlers",
                schema: "public",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    handler_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_processed_domain_event_handlers", x => new { x.event_id, x.handler_name });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "processed_domain_event_handlers",
                schema: "public");
        }
    }
}
