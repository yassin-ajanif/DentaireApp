using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentaireApp.DataAccess.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class EnforceAppointmentPatientForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure strict FK can be applied even if legacy orphan rows exist.
            migrationBuilder.Sql(
                """
                DELETE FROM Appointments
                WHERE PatientId NOT IN (SELECT Id FROM Patients);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Patients_PatientId",
                table: "Appointments",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Patients_PatientId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments");
        }
    }
}
