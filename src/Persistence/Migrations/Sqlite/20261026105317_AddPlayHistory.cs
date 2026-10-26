using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MariesWonderland.Persistence.Migrations.Sqlite
{
    /// <summary>Adds the server-only play-history counters (metres walked in The Cage for the smartphone's pedometer).</summary>
    public partial class AddPlayHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SPlayHistory",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    GamePlayHistoryType = table.Column<int>(type: "INTEGER", nullable: false),
                    Count = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SPlayHistory", x => new { x.UserId, x.GamePlayHistoryType });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SPlayHistory");
        }
    }
}
