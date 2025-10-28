using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Domain.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserDeleteBehaviour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entries_AspNetUsers_UserId",
                table: "Entries");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_AspNetUsers_PatientId",
                table: "Reviews");

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_AspNetUsers_UserId",
                table: "Entries",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_AspNetUsers_PatientId",
                table: "Reviews",
                column: "PatientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entries_AspNetUsers_UserId",
                table: "Entries");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_AspNetUsers_PatientId",
                table: "Reviews");

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_AspNetUsers_UserId",
                table: "Entries",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_AspNetUsers_PatientId",
                table: "Reviews",
                column: "PatientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
