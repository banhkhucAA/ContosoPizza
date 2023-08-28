using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContosoPizza.Migrations
{
    public partial class UpdateStatusTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedCancelledAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDeActiveAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDeliveredAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDeliveringAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedMakingAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedReturnedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedWaitingAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedCancelledAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UpdatedDeActiveAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UpdatedDeliveredAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UpdatedDeliveringAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UpdatedMakingAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UpdatedReturnedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UpdatedWaitingAt",
                table: "Orders");
        }
    }
}
