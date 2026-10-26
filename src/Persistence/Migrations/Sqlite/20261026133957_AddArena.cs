using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MariesWonderland.Persistence.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddArena : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArenaBattles",
                columns: table => new
                {
                    BattleId = table.Column<long>(type: "INTEGER", nullable: false),
                    SeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    AttackerUserId = table.Column<long>(type: "INTEGER", nullable: false),
                    DefenderUserId = table.Column<long>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    FinishedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    AttackerWon = table.Column<bool>(type: "INTEGER", nullable: false),
                    AttackerPointsChange = table.Column<int>(type: "INTEGER", nullable: false),
                    DefenderPointsChange = table.Column<int>(type: "INTEGER", nullable: false),
                    AttackerDeckPower = table.Column<int>(type: "INTEGER", nullable: false),
                    AttackerCostumeIds = table.Column<string>(type: "TEXT", nullable: false),
                    DefenderDeckPower = table.Column<int>(type: "INTEGER", nullable: false),
                    DefenderCostumeIds = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArenaBattles", x => x.BattleId);
                });

            migrationBuilder.CreateTable(
                name: "ArenaEntries",
                columns: table => new
                {
                    SeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    PlayerId = table.Column<long>(type: "INTEGER", nullable: false),
                    IsBot = table.Column<bool>(type: "INTEGER", nullable: false),
                    SeasonEndsAt = table.Column<long>(type: "INTEGER", nullable: false),
                    Points = table.Column<int>(type: "INTEGER", nullable: false),
                    PointsChangedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    AttackWins = table.Column<int>(type: "INTEGER", nullable: false),
                    AttackLosses = table.Column<int>(type: "INTEGER", nullable: false),
                    AttackPointsChange = table.Column<int>(type: "INTEGER", nullable: false),
                    DefenseWins = table.Column<int>(type: "INTEGER", nullable: false),
                    DefenseLosses = table.Column<int>(type: "INTEGER", nullable: false),
                    DefensePointsChange = table.Column<int>(type: "INTEGER", nullable: false),
                    DefenseDeckPower = table.Column<int>(type: "INTEGER", nullable: false),
                    DefenseCostumeIds = table.Column<string>(type: "TEXT", nullable: false),
                    DefenseAttributeTypes = table.Column<string>(type: "TEXT", nullable: false),
                    MostPowerfulCostumeId = table.Column<int>(type: "INTEGER", nullable: false),
                    MatchingUserIds = table.Column<string>(type: "TEXT", nullable: false),
                    BotName = table.Column<string>(type: "TEXT", nullable: false),
                    BotSeed = table.Column<int>(type: "INTEGER", nullable: false),
                    BotFavoriteCostumeId = table.Column<int>(type: "INTEGER", nullable: false),
                    SeasonFinalRank = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArenaEntries", x => new { x.SeasonId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "ArenaWeeks",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    WeekStart = table.Column<long>(type: "INTEGER", nullable: false),
                    SeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    FinalPoint = table.Column<int>(type: "INTEGER", nullable: false),
                    FinalRank = table.Column<int>(type: "INTEGER", nullable: false),
                    Closed = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArenaWeeks", x => new { x.UserId, x.WeekStart });
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArenaBattles_SeasonId",
                table: "ArenaBattles",
                column: "SeasonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArenaBattles");

            migrationBuilder.DropTable(
                name: "ArenaEntries");

            migrationBuilder.DropTable(
                name: "ArenaWeeks");
        }
    }
}
