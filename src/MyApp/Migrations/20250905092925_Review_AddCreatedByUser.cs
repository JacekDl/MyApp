using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Migrations
{
    /// <inheritdoc />
    public partial class Review_AddCreatedByUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Reviews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_CreatedByUserId",
                table: "Reviews",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_CreatedByUserId",
                table: "Reviews",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_CreatedByUserId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_CreatedByUserId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Reviews");
        }
    }
}
