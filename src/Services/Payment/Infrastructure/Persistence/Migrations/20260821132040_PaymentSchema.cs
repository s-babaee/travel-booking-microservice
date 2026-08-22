using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PaymentSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payment_transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuthorizedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RefundedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_transactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "refunds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refunds", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_BookingId",
                table: "payment_transactions",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_IdempotencyKey",
                table: "payment_transactions",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_UserId",
                table: "payment_transactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_refunds_BookingId",
                table: "refunds",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_refunds_PaymentId",
                table: "refunds",
                column: "PaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refunds_UserId",
                table: "refunds",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment_transactions");

            migrationBuilder.DropTable(
                name: "refunds");
        }
    }
}
