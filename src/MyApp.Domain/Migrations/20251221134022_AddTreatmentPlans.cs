using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddTreatmentPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TreatmentPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateCompleted = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IdPharmacist = table.Column<string>(type: "TEXT", nullable: true),
                    IdPatient = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentPlans_AspNetUsers_IdPatient",
                        column: x => x.IdPatient,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TreatmentPlans_AspNetUsers_IdPharmacist",
                        column: x => x.IdPharmacist,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentPlanAdvices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdTreatmentPlan = table.Column<int>(type: "INTEGER", nullable: false),
                    AdviceText = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentPlanAdvices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentPlanAdvices_TreatmentPlans_IdTreatmentPlan",
                        column: x => x.IdTreatmentPlan,
                        principalTable: "TreatmentPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentPlanMedicines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdTreatmentPlan = table.Column<int>(type: "INTEGER", nullable: false),
                    MedicineName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Dosage = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TimeOfDay = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentPlanMedicines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentPlanMedicines_TreatmentPlans_IdTreatmentPlan",
                        column: x => x.IdTreatmentPlan,
                        principalTable: "TreatmentPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicineTakenConfirmations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdTreatmentPlanMedicine = table.Column<int>(type: "INTEGER", nullable: false),
                    DateTimeTaken = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicineTakenConfirmations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicineTakenConfirmations_TreatmentPlanMedicines_IdTreatmentPlanMedicine",
                        column: x => x.IdTreatmentPlanMedicine,
                        principalTable: "TreatmentPlanMedicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicineTakenConfirmations_IdTreatmentPlanMedicine",
                table: "MedicineTakenConfirmations",
                column: "IdTreatmentPlanMedicine");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentPlanAdvices_IdTreatmentPlan",
                table: "TreatmentPlanAdvices",
                column: "IdTreatmentPlan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentPlanMedicines_IdTreatmentPlan",
                table: "TreatmentPlanMedicines",
                column: "IdTreatmentPlan");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentPlans_IdPatient",
                table: "TreatmentPlans",
                column: "IdPatient");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentPlans_IdPharmacist",
                table: "TreatmentPlans",
                column: "IdPharmacist");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicineTakenConfirmations");

            migrationBuilder.DropTable(
                name: "TreatmentPlanAdvices");

            migrationBuilder.DropTable(
                name: "TreatmentPlanMedicines");

            migrationBuilder.DropTable(
                name: "TreatmentPlans");
        }
    }
}
