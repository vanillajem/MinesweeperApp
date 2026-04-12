using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinesweeperApp.Migrations
{
    /// <inheritdoc />
    public partial class AddGameScoresTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "GameScores",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Score",
                table: "GameScores");
        }
    }
}
