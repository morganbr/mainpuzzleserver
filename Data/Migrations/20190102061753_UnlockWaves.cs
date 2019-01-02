using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class UnlockWaves : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnlockWaveID",
                table: "Puzzles",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UnlockWave",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: false),
                    EventID = table.Column<int>(nullable: false),
                    PreviousWaveId = table.Column<int>(nullable: true),
                    TimeAfterPreviousWave = table.Column<TimeSpan>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnlockWave", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnlockWave_Events_EventID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UnlockWave_UnlockWave_PreviousWaveId",
                        column: x => x.PreviousWaveId,
                        principalTable: "UnlockWave",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnlockWavePerTeam",
                columns: table => new
                {
                    WaveID = table.Column<int>(nullable: false),
                    TeamID = table.Column<int>(nullable: false),
                    TimeUnlocked = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnlockWavePerTeam", x => new { x.WaveID, x.TeamID });
                    table.ForeignKey(
                        name: "FK_UnlockWavePerTeam_Teams_TeamID",
                        column: x => x.TeamID,
                        principalTable: "Teams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UnlockWavePerTeam_UnlockWave_WaveID",
                        column: x => x.WaveID,
                        principalTable: "UnlockWave",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Puzzles_UnlockWaveID",
                table: "Puzzles",
                column: "UnlockWaveID");

            migrationBuilder.CreateIndex(
                name: "IX_UnlockWave_EventID",
                table: "UnlockWave",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_UnlockWave_PreviousWaveId",
                table: "UnlockWave",
                column: "PreviousWaveId");

            migrationBuilder.CreateIndex(
                name: "IX_UnlockWavePerTeam_TeamID",
                table: "UnlockWavePerTeam",
                column: "TeamID");

            migrationBuilder.AddForeignKey(
                name: "FK_Puzzles_UnlockWave_UnlockWaveID",
                table: "Puzzles",
                column: "UnlockWaveID",
                principalTable: "UnlockWave",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Puzzles_UnlockWave_UnlockWaveID",
                table: "Puzzles");

            migrationBuilder.DropTable(
                name: "UnlockWavePerTeam");

            migrationBuilder.DropTable(
                name: "UnlockWave");

            migrationBuilder.DropIndex(
                name: "IX_Puzzles_UnlockWaveID",
                table: "Puzzles");

            migrationBuilder.DropColumn(
                name: "UnlockWaveID",
                table: "Puzzles");
        }
    }
}
