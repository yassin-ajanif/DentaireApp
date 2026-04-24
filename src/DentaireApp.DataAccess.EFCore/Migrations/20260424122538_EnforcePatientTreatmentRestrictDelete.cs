using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentaireApp.DataAccess.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class EnforcePatientTreatmentRestrictDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentInfos_Patients_PatientId",
                table: "TreatmentInfos");

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentInfos_Patients_PatientId",
                table: "TreatmentInfos",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentInfos_Patients_PatientId",
                table: "TreatmentInfos");

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentInfos_Patients_PatientId",
                table: "TreatmentInfos",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
