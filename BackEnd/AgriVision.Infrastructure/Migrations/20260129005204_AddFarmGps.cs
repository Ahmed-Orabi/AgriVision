using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriVision.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFarmGps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Farms",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Length",
                table: "Farms",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "SoilType",
                table: "Farms",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Width",
                table: "Farms",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "FarmFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Crop = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    XStart = table.Column<double>(type: "float", nullable: false),
                    XEnd = table.Column<double>(type: "float", nullable: false),
                    YStart = table.Column<double>(type: "float", nullable: false),
                    YEnd = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Healthy"),
                    Temperature = table.Column<double>(type: "float", nullable: true),
                    Moisture = table.Column<double>(type: "float", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarmFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FarmFields_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FarmFields_FarmId",
                table: "FarmFields",
                column: "FarmId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FarmFields");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "Length",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "SoilType",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "Farms");
        }
    }
}
