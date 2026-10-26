using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MariesWonderland.Persistence.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddArenaBattleDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ArenaBattleId",
                table: "SBattleDetail",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArenaBattleId",
                table: "SBattleDetail");
        }
    }
}
