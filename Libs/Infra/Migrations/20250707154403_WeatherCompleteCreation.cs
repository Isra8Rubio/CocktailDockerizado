using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Weather.infra.Migrations
{
    /// <inheritdoc />
    public partial class WeatherCompleteCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxTemperature",
                table: "WeatherComplete",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinTemperature",
                table: "WeatherComplete",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NameTown",
                table: "WeatherComplete",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StateSkyDescription",
                table: "WeatherComplete",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StateSkyId",
                table: "WeatherComplete",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxTemperature",
                table: "WeatherComplete");

            migrationBuilder.DropColumn(
                name: "MinTemperature",
                table: "WeatherComplete");

            migrationBuilder.DropColumn(
                name: "NameTown",
                table: "WeatherComplete");

            migrationBuilder.DropColumn(
                name: "StateSkyDescription",
                table: "WeatherComplete");

            migrationBuilder.DropColumn(
                name: "StateSkyId",
                table: "WeatherComplete");
        }
    }
}
