using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MariesWonderland.Persistence.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddQuestSessionDeckType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserDeckType",
                table: "SQuestSession",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserDeckType",
                table: "SQuestSession");
        }
    }
}
