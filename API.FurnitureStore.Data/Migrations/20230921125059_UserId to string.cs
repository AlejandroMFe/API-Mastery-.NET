using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.FurnitureStore.Data.Migrations
{
    public partial class UserIdtostring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "RefreshTokens",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "RefreshTokens",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}
