using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Domain.Migrations
{
    /// <inheritdoc />
    public partial class TreatmentPlanStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TreatmentPlans",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE TreatmentPlans
                SET Status = CASE WHEN Claimed = 1 THEN 1 ELSE 0 END
            ");

            migrationBuilder.DropColumn(
                name: "Claimed",
                table: "TreatmentPlans");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "TreatmentPlans");

            migrationBuilder.AddColumn<bool>(
                name: "Claimed",
                table: "TreatmentPlans",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(@"
                UPDATE TreatmentPlans
                SET Claimed = CASE WHEN Status = 1 THEN 1 ELSE 0 END
            ");
        }
    }
}
