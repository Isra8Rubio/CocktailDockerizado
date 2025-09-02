using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Weather.infra.Migrations
{
    /// <inheritdoc />
    public partial class MinimalWeather : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "WeatherComplete");

            migrationBuilder.DropColumn(
                name: "Humidity",
                table: "WeatherComplete");

            migrationBuilder.DropColumn(
                name: "IdTown",
                table: "WeatherComplete");

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
                name: "Precipitation",
                table: "WeatherComplete");

            migrationBuilder.DropColumn(
                name: "Rain",
                table: "WeatherComplete");

            migrationBuilder.DropColumn(
                name: "StateSkyDescription",
                table: "WeatherComplete");

            migrationBuilder.DropColumn(
                name: "StateSkyId",
                table: "WeatherComplete");

            migrationBuilder.DropColumn(
                name: "Temperature",
                table: "WeatherComplete");

            migrationBuilder.DropColumn(
                name: "Wind",
                table: "WeatherComplete");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "WeatherComplete",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<int>(
                name: "Humidity",
                table: "WeatherComplete",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IdTown",
                table: "WeatherComplete",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AddColumn<int>(
                name: "Precipitation",
                table: "WeatherComplete",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Rain",
                table: "WeatherComplete",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.AddColumn<int>(
                name: "Temperature",
                table: "WeatherComplete",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Wind",
                table: "WeatherComplete",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
