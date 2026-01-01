using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TreatmentPlanReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdTreatmentPlan = table.Column<int>(type: "INTEGER", nullable: false),
                    UnreadForPharmacist = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    UnreadForPatient = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentPlanReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentPlanReviews_TreatmentPlans_IdTreatmentPlan",
                        column: x => x.IdTreatmentPlan,
                        principalTable: "TreatmentPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdTreatmentPlanReview = table.Column<int>(type: "INTEGER", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Author = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewEntries_TreatmentPlanReviews_IdTreatmentPlanReview",
                        column: x => x.IdTreatmentPlanReview,
                        principalTable: "TreatmentPlanReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewEntries_IdTreatmentPlanReview_DateCreated",
                table: "ReviewEntries",
                columns: new[] { "IdTreatmentPlanReview", "DateCreated" });

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentPlanReviews_IdTreatmentPlan",
                table: "TreatmentPlanReviews",
                column: "IdTreatmentPlan",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewEntries");

            migrationBuilder.DropTable(
                name: "TreatmentPlanReviews");
        }
    }
}
