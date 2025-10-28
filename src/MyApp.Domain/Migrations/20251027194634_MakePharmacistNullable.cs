using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Domain.Migrations
{
    /// <inheritdoc />
    public partial class MakePharmacistNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_AspNetUsers_PharmacistId",
                table: "Reviews");

            migrationBuilder.AlterColumn<string>(
                name: "PharmacistId",
                table: "Reviews",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_AspNetUsers_PharmacistId",
                table: "Reviews",
                column: "PharmacistId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_AspNetUsers_PharmacistId",
                table: "Reviews");

            migrationBuilder.AlterColumn<string>(
                name: "PharmacistId",
                table: "Reviews",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_AspNetUsers_PharmacistId",
                table: "Reviews",
                column: "PharmacistId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
