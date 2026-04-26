using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentaireApp.DataAccess.EFCore.Migrations;

/// <inheritdoc />
public partial class StoreAppointmentTimesAsIsoText : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // SQLite: INTEGER unix → readable UTC ISO-8601 TEXT (rebuild table; type change is not a simple ALTER).
        migrationBuilder.Sql(
            """
            PRAGMA foreign_keys = 0;

            CREATE TABLE "Appointments_new" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_Appointments" PRIMARY KEY,
                "PatientId" TEXT NOT NULL,
                "QueueNumber" INTEGER NOT NULL,
                "Status" INTEGER NOT NULL,
                "StartedAt" TEXT NULL,
                "CompletedAt" TEXT NULL,
                CONSTRAINT "FK_Appointments_Patients_PatientId" FOREIGN KEY ("PatientId") REFERENCES "Patients" ("Id") ON DELETE RESTRICT
            );

            INSERT INTO "Appointments_new" ("Id", "PatientId", "QueueNumber", "Status", "StartedAt", "CompletedAt")
            SELECT
                "Id",
                "PatientId",
                "QueueNumber",
                "Status",
                CASE
                    WHEN "StartedAt" IS NULL THEN NULL
                    ELSE strftime('%Y-%m-%dT%H:%M:%SZ', datetime("StartedAt", 'unixepoch'))
                END,
                CASE
                    WHEN "CompletedAt" IS NULL THEN NULL
                    ELSE strftime('%Y-%m-%dT%H:%M:%SZ', datetime("CompletedAt", 'unixepoch'))
                END
            FROM "Appointments";

            DROP TABLE "Appointments";

            ALTER TABLE "Appointments_new" RENAME TO "Appointments";

            CREATE INDEX "IX_Appointments_PatientId" ON "Appointments" ("PatientId");

            PRAGMA foreign_keys = 1;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            PRAGMA foreign_keys = 0;

            CREATE TABLE "Appointments_new" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_Appointments" PRIMARY KEY,
                "PatientId" TEXT NOT NULL,
                "QueueNumber" INTEGER NOT NULL,
                "Status" INTEGER NOT NULL,
                "StartedAt" INTEGER NULL,
                "CompletedAt" INTEGER NULL,
                CONSTRAINT "FK_Appointments_Patients_PatientId" FOREIGN KEY ("PatientId") REFERENCES "Patients" ("Id") ON DELETE RESTRICT
            );

            INSERT INTO "Appointments_new" ("Id", "PatientId", "QueueNumber", "Status", "StartedAt", "CompletedAt")
            SELECT
                "Id",
                "PatientId",
                "QueueNumber",
                "Status",
                CASE
                    WHEN "StartedAt" IS NULL OR trim("StartedAt") = '' THEN NULL
                    ELSE CAST(strftime('%s', "StartedAt") AS INTEGER)
                END,
                CASE
                    WHEN "CompletedAt" IS NULL OR trim("CompletedAt") = '' THEN NULL
                    ELSE CAST(strftime('%s', "CompletedAt") AS INTEGER)
                END
            FROM "Appointments";

            DROP TABLE "Appointments";

            ALTER TABLE "Appointments_new" RENAME TO "Appointments";

            CREATE INDEX "IX_Appointments_PatientId" ON "Appointments" ("PatientId");

            PRAGMA foreign_keys = 1;
            """);
    }
}
