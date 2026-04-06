using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shipment.Migrations
{
    /// <inheritdoc />
    public partial class NewShipmentMigratio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shipments_NotifyStartAt_TimeOfArrival_LastNotifiedAt_IsComp~",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_TimeOfArrival",
                table: "Shipments");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NotifyStartAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastNotifiedAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_IsCompleted_LastNotifiedAt_TimeOfArrival",
                table: "Shipments",
                columns: new[] { "IsCompleted", "LastNotifiedAt", "TimeOfArrival" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shipments_IsCompleted_LastNotifiedAt_TimeOfArrival",
                table: "Shipments");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NotifyStartAt",
                table: "Shipments",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastNotifiedAt",
                table: "Shipments",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_NotifyStartAt_TimeOfArrival_LastNotifiedAt_IsComp~",
                table: "Shipments",
                columns: new[] { "NotifyStartAt", "TimeOfArrival", "LastNotifiedAt", "IsCompleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_TimeOfArrival",
                table: "Shipments",
                column: "TimeOfArrival");
        }
    }
}
