using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContosoPizza.Migrations
{
    public partial class CustomerAddressMax : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Customers",
                type: "varchar(max)",
                unicode: false,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldUnicode: false,
                oldMaxLength: 30);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Customers",
                type: "varchar(30)",
                unicode: false,
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(max)",
                oldUnicode: false);
        }
    }
}
