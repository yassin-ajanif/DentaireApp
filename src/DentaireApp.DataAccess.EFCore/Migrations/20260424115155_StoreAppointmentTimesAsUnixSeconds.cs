using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentaireApp.DataAccess.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class StoreAppointmentTimesAsUnixSeconds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert existing SQLite TEXT datetimes to unix seconds before changing column mapping.
            migrationBuilder.Sql(
                """
                UPDATE Appointments
                SET StartedAt = CASE
                    WHEN StartedAt IS NULL THEN NULL
                    ELSE CAST(strftime('%s', StartedAt) AS INTEGER)
                END;
                """);
            migrationBuilder.Sql(
                """
                UPDATE Appointments
                SET CompletedAt = CASE
                    WHEN CompletedAt IS NULL THEN NULL
                    ELSE CAST(strftime('%s', CompletedAt) AS INTEGER)
                END;
                """);

            migrationBuilder.AlterColumn<long>(
                name: "StartedAt",
                table: "Appointments",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "CompletedAt",
                table: "Appointments",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Convert unix seconds back to SQLite TEXT datetime on rollback.
            migrationBuilder.Sql(
                """
                UPDATE Appointments
                SET StartedAt = CASE
                    WHEN StartedAt IS NULL THEN NULL
                    ELSE datetime(StartedAt, 'unixepoch')
                END;
                """);
            migrationBuilder.Sql(
                """
                UPDATE Appointments
                SET CompletedAt = CASE
                    WHEN CompletedAt IS NULL THEN NULL
                    ELSE datetime(CompletedAt, 'unixepoch')
                END;
                """);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                table: "Appointments",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedAt",
                table: "Appointments",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
