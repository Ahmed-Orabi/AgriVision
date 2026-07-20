using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriVision.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFertilizerRecommendationHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FertilizerRecommendationHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Temperature = table.Column<double>(type: "float", nullable: false),
                    Moisture = table.Column<double>(type: "float", nullable: false),
                    Rainfall = table.Column<double>(type: "float", nullable: false),
                    PH = table.Column<double>(type: "float", nullable: false),
                    Nitrogen = table.Column<double>(type: "float", nullable: false),
                    Phosphorous = table.Column<double>(type: "float", nullable: false),
                    Potassium = table.Column<double>(type: "float", nullable: false),
                    Carbon = table.Column<double>(type: "float", nullable: false),
                    Soil = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Crop = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Fertilizer1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Confidence1 = table.Column<double>(type: "float", nullable: false),
                    Fertilizer2 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Confidence2 = table.Column<double>(type: "float", nullable: false),
                    Fertilizer3 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Confidence3 = table.Column<double>(type: "float", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FertilizerRecommendationHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FertilizerRecommendationHistories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FertilizerRecommendationHistories_UserId",
                table: "FertilizerRecommendationHistories",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FertilizerRecommendationHistories");
        }
    }
}
