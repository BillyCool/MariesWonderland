using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MariesWonderland.Persistence.Migrations.Sqlite
{
    /// <summary>
    /// Moves the birth year and month from IUser (a client table, which the client has no such fields for) to the
    /// server-only SUser table, keeping any values already saved.
    /// </summary>
    public partial class MoveBirthDateToServerUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BirthMonth",
                table: "SUser",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BirthYear",
                table: "SUser",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            // Copy the saved values before the old columns go: update existing SUser rows, create missing ones.
            migrationBuilder.Sql(
                """
                UPDATE SUser
                SET BirthYear = (SELECT IUser.BirthYear FROM IUser WHERE IUser.UserId = SUser.UserId),
                    BirthMonth = (SELECT IUser.BirthMonth FROM IUser WHERE IUser.UserId = SUser.UserId)
                WHERE EXISTS (SELECT 1 FROM IUser WHERE IUser.UserId = SUser.UserId);
                """);
            migrationBuilder.Sql(
                """
                INSERT INTO SUser (UserId, BackupToken, ChargeMoneyThisMonth, BirthYear, BirthMonth)
                SELECT UserId, '', 0, BirthYear, BirthMonth FROM IUser
                WHERE (BirthYear <> 0 OR BirthMonth <> 0)
                  AND UserId NOT IN (SELECT UserId FROM SUser);
                """);

            migrationBuilder.DropColumn(
                name: "BirthMonth",
                table: "IUser");

            migrationBuilder.DropColumn(
                name: "BirthYear",
                table: "IUser");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BirthMonth",
                table: "IUser",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BirthYear",
                table: "IUser",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE IUser
                SET BirthYear = (SELECT SUser.BirthYear FROM SUser WHERE SUser.UserId = IUser.UserId),
                    BirthMonth = (SELECT SUser.BirthMonth FROM SUser WHERE SUser.UserId = IUser.UserId)
                WHERE EXISTS (SELECT 1 FROM SUser WHERE SUser.UserId = IUser.UserId);
                """);

            migrationBuilder.DropColumn(
                name: "BirthMonth",
                table: "SUser");

            migrationBuilder.DropColumn(
                name: "BirthYear",
                table: "SUser");
        }
    }
}
