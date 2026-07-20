using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriVision.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCropRecommendationHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CropRecommendationHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    N = table.Column<int>(type: "int", nullable: false),
                    P = table.Column<int>(type: "int", nullable: false),
                    K = table.Column<int>(type: "int", nullable: false),
                    Temperature = table.Column<double>(type: "float", nullable: false),
                    Humidity = table.Column<double>(type: "float", nullable: false),
                    Ph = table.Column<double>(type: "float", nullable: false),
                    Rainfall = table.Column<double>(type: "float", nullable: false),
                    Crop1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Confidence1 = table.Column<double>(type: "float", nullable: false),
                    Crop2 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Confidence2 = table.Column<double>(type: "float", nullable: false),
                    Crop3 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Confidence3 = table.Column<double>(type: "float", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropRecommendationHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CropRecommendationHistories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CropRecommendationHistories_UserId_CreatedAtUtc",
                table: "CropRecommendationHistories",
                columns: new[] { "UserId", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CropRecommendationHistories");
        }
    }
}
