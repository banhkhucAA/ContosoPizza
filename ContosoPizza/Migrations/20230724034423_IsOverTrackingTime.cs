using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContosoPizza.Migrations
{
    public partial class IsOverTrackingTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOverDeliveringTime",
                table: "Orders",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOverMakingTime",
                table: "Orders",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOverWaitingTime",
                table: "Orders",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOverDeliveringTime",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsOverMakingTime",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsOverWaitingTime",
                table: "Orders");
        }
    }
}
