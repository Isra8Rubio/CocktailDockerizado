using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Weather.infra.Migrations
{
    /// <inheritdoc />
    public partial class HostedService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeatherComplete");

            migrationBuilder.CreateTable(
                name: "RandomCocktails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DrinkId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThumbUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RandomCocktails", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RandomCocktails");

            migrationBuilder.CreateTable(
                name: "WeatherComplete",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdProvince = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxTemperature = table.Column<int>(type: "int", nullable: true),
                    MinTemperature = table.Column<int>(type: "int", nullable: true),
                    NameProvince = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameTown = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StateSkyDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StateSkyId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherComplete", x => x.Id);
                });
        }
    }
}
