using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "flight_inventory_days",
                columns: table => new
                {
                    flight_id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_date = table.Column<DateOnly>(type: "date", nullable: false),
                    total_seats = table.Column<int>(type: "integer", nullable: false),
                    available_seats = table.Column<int>(type: "integer", nullable: false),
                    held_seats = table.Column<int>(type: "integer", nullable: false),
                    confirmed_seats = table.Column<int>(type: "integer", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_inventory_days", x => new { x.flight_id, x.flight_class_id, x.inventory_date });
                });

            migrationBuilder.CreateTable(
                name: "flight_inventory_holds",
                columns: table => new
                {
                    hold_id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_inventory_holds", x => x.hold_id);
                });

            migrationBuilder.CreateTable(
                name: "hotel_inventory_days",
                columns: table => new
                {
                    hotel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    room_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_date = table.Column<DateOnly>(type: "date", nullable: false),
                    total_units = table.Column<int>(type: "integer", nullable: false),
                    available_units = table.Column<int>(type: "integer", nullable: false),
                    held_units = table.Column<int>(type: "integer", nullable: false),
                    confirmed_units = table.Column<int>(type: "integer", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_inventory_days", x => new { x.hotel_id, x.room_type_id, x.inventory_date });
                });

            migrationBuilder.CreateTable(
                name: "hotel_inventory_holds",
                columns: table => new
                {
                    hold_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hotel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_inventory_holds", x => x.hold_id);
                });

            migrationBuilder.CreateTable(
                name: "flight_inventory_hold_lines",
                columns: table => new
                {
                    flight_class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_date = table.Column<DateOnly>(type: "date", nullable: false),
                    hold_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_inventory_hold_lines", x => new { x.hold_id, x.flight_class_id, x.inventory_date });
                    table.ForeignKey(
                        name: "FK_flight_inventory_hold_lines_flight_inventory_holds_hold_id",
                        column: x => x.hold_id,
                        principalTable: "flight_inventory_holds",
                        principalColumn: "hold_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hotel_inventory_hold_lines",
                columns: table => new
                {
                    room_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_date = table.Column<DateOnly>(type: "date", nullable: false),
                    hold_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_inventory_hold_lines", x => new { x.hold_id, x.room_type_id, x.inventory_date });
                    table.ForeignKey(
                        name: "FK_hotel_inventory_hold_lines_hotel_inventory_holds_hold_id",
                        column: x => x.hold_id,
                        principalTable: "hotel_inventory_holds",
                        principalColumn: "hold_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_flight_inventory_days_flight_id_inventory_date",
                table: "flight_inventory_days",
                columns: new[] { "flight_id", "inventory_date" });

            migrationBuilder.CreateIndex(
                name: "IX_flight_inventory_holds_status_expires_at_utc",
                table: "flight_inventory_holds",
                columns: new[] { "status", "expires_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_hotel_inventory_days_hotel_id_inventory_date",
                table: "hotel_inventory_days",
                columns: new[] { "hotel_id", "inventory_date" });

            migrationBuilder.CreateIndex(
                name: "IX_hotel_inventory_holds_status_expires_at_utc",
                table: "hotel_inventory_holds",
                columns: new[] { "status", "expires_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "flight_inventory_days");

            migrationBuilder.DropTable(
                name: "flight_inventory_hold_lines");

            migrationBuilder.DropTable(
                name: "hotel_inventory_days");

            migrationBuilder.DropTable(
                name: "hotel_inventory_hold_lines");

            migrationBuilder.DropTable(
                name: "flight_inventory_holds");

            migrationBuilder.DropTable(
                name: "hotel_inventory_holds");
        }
    }
}
