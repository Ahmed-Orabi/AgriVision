using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriVision.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFarmUnitsAndSensors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Humidity",
                table: "Farms",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LengthUnit",
                table: "Farms",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "m");

            migrationBuilder.AddColumn<double>(
                name: "Temperature",
                table: "Farms",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SoilMoisture",
                table: "FarmFields",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SoilTemperature",
                table: "FarmFields",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Humidity",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "LengthUnit",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "Temperature",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "SoilMoisture",
                table: "FarmFields");

            migrationBuilder.DropColumn(
                name: "SoilTemperature",
                table: "FarmFields");
        }
    }
}
