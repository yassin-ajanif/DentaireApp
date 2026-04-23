using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentaireApp.DataAccess.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class UniquePatientTelephone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Patients_Nom_Telephone",
                table: "Patients");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_Telephone",
                table: "Patients",
                column: "Telephone",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Patients_Telephone",
                table: "Patients");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_Nom_Telephone",
                table: "Patients",
                columns: new[] { "Nom", "Telephone" },
                unique: true);
        }
    }
}
