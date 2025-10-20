using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddModifiedField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PatientModified",
                table: "Reviews",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PharmacistModified",
                table: "Reviews",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PatientModified",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "PharmacistModified",
                table: "Reviews");
        }
    }
}
