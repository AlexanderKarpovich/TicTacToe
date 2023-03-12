using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicTacToe.Api.Migrations
{
    public partial class GamesInitial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Variant = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.PlayerId);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    GameSessionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Player1PlayerId = table.Column<int>(type: "INTEGER", nullable: true),
                    Player2PlayerId = table.Column<int>(type: "INTEGER", nullable: true),
                    PlayerTurnPlayerId = table.Column<int>(type: "INTEGER", nullable: true),
                    WinnerPlayerId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsEmpty = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.GameSessionId);
                    table.ForeignKey(
                        name: "FK_Games_Players_Player1PlayerId",
                        column: x => x.Player1PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId");
                    table.ForeignKey(
                        name: "FK_Games_Players_Player2PlayerId",
                        column: x => x.Player2PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId");
                    table.ForeignKey(
                        name: "FK_Games_Players_PlayerTurnPlayerId",
                        column: x => x.PlayerTurnPlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId");
                    table.ForeignKey(
                        name: "FK_Games_Players_WinnerPlayerId",
                        column: x => x.WinnerPlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId");
                });

            migrationBuilder.CreateTable(
                name: "GameCells",
                columns: table => new
                {
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    GameSessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Variant = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameCells", x => new { x.GameSessionId, x.Position });
                    table.ForeignKey(
                        name: "FK_GameCells_Games_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "Games",
                        principalColumn: "GameSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Games_Player1PlayerId",
                table: "Games",
                column: "Player1PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_Player2PlayerId",
                table: "Games",
                column: "Player2PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_PlayerTurnPlayerId",
                table: "Games",
                column: "PlayerTurnPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_WinnerPlayerId",
                table: "Games",
                column: "WinnerPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_Name",
                table: "Players",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameCells");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
