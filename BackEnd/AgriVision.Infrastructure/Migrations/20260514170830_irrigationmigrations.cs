using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriVision.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class irrigationmigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IrrigationRecommendationHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    gSoil_type = table.Column<int>(type: "int", nullable: false),
                    Soil_pH = table.Column<float>(type: "real", nullable: false),
                    Soil_Moisture = table.Column<float>(type: "real", nullable: false),
                    Electrical_Conductivity = table.Column<float>(type: "real", nullable: false),
                    Temperature_C = table.Column<float>(type: "real", nullable: false),
                    Humidity = table.Column<float>(type: "real", nullable: false),
                    Rainfall_mm = table.Column<float>(type: "real", nullable: false),
                    Crop_Type = table.Column<int>(type: "int", nullable: false),
                    Crop_Growth_Stage = table.Column<int>(type: "int", nullable: false),
                    Season = table.Column<int>(type: "int", nullable: false),
                    Field_Area_hectare = table.Column<float>(type: "real", nullable: false),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    predicted_irrigation_need = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IrrigationRecommendationHistories", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IrrigationRecommendationHistories");
        }
    }
}
