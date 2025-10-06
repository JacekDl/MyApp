using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Application.Migrations
{
    /// <inheritdoc />
    public partial class RemovePharmacyFieldsFromUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PharmacyCity",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PharmacyName",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PharmacyCity",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PharmacyName",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 32,
                nullable: true);
        }
    }
}
