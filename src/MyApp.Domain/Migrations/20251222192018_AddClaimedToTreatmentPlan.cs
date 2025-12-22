using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimedToTreatmentPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Claimed",
                table: "TreatmentPlans",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Claimed",
                table: "TreatmentPlans");
        }
    }
}
