using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TimestampUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AuthenticationMethod = table.Column<string>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: true),
                    ComputerName = table.Column<string>(type: "TEXT", nullable: false),
                    AttemptsInWindow = table.Column<int>(type: "INTEGER", nullable: false),
                    CaptureSucceeded = table.Column<bool>(type: "INTEGER", nullable: false),
                    CaptureFailureReason = table.Column<string>(type: "TEXT", nullable: true),
                    EvidenceFolder = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_TimestampUtc",
                table: "Incidents",
                column: "TimestampUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Incidents");
        }
    }
}
