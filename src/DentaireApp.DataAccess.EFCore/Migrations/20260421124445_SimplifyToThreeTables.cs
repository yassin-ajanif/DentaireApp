using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentaireApp.DataAccess.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyToThreeTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS TreatmentLines;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS TreatmentSheets;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS TreatmentRecords;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS InvoiceLines;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS Payments;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS Invoices;");

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PatientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    QueueNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Age = table.Column<int>(type: "INTEGER", nullable: false),
                    Telephone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Adresse = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PatientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NatureOperation = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    PrixConven = table.Column<decimal>(type: "TEXT", nullable: false),
                    Recu = table.Column<decimal>(type: "TEXT", nullable: false),
                    ARecevoir = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentInfos_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patients_Nom_Telephone",
                table: "Patients",
                columns: new[] { "Nom", "Telephone" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentInfos_PatientId_Date",
                table: "TreatmentInfos",
                columns: new[] { "PatientId", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TreatmentInfos");
        }
    }
}
