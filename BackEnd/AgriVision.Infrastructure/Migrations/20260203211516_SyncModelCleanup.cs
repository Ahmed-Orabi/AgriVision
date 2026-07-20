using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriVision.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_insectDetections_AspNetUsers_UserId",
                table: "insectDetections");

            migrationBuilder.DropForeignKey(
                name: "FK_notifications_AspNetUsers_UserId",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_plantDetections_AspNetUsers_UserId",
                table: "plantDetections");

            migrationBuilder.DropPrimaryKey(
                name: "PK_plantDetections",
                table: "plantDetections");

            migrationBuilder.DropPrimaryKey(
                name: "PK_notifications",
                table: "notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_insectDetections",
                table: "insectDetections");

            migrationBuilder.RenameTable(
                name: "plantDetections",
                newName: "PlantDetections");

            migrationBuilder.RenameTable(
                name: "notifications",
                newName: "Notifications");

            migrationBuilder.RenameTable(
                name: "insectDetections",
                newName: "InsectDetections");

            migrationBuilder.RenameIndex(
                name: "IX_plantDetections_UserId",
                table: "PlantDetections",
                newName: "IX_PlantDetections_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_notifications_UserId",
                table: "Notifications",
                newName: "IX_Notifications_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_insectDetections_UserId",
                table: "InsectDetections",
                newName: "IX_InsectDetections_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlantDetections",
                table: "PlantDetections",
                column: "PlantDetectionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InsectDetections",
                table: "InsectDetections",
                column: "InsectDetectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_InsectDetections_AspNetUsers_UserId",
                table: "InsectDetections",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlantDetections_AspNetUsers_UserId",
                table: "PlantDetections",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InsectDetections_AspNetUsers_UserId",
                table: "InsectDetections");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_UserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_PlantDetections_AspNetUsers_UserId",
                table: "PlantDetections");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlantDetections",
                table: "PlantDetections");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InsectDetections",
                table: "InsectDetections");

            migrationBuilder.RenameTable(
                name: "PlantDetections",
                newName: "plantDetections");

            migrationBuilder.RenameTable(
                name: "Notifications",
                newName: "notifications");

            migrationBuilder.RenameTable(
                name: "InsectDetections",
                newName: "insectDetections");

            migrationBuilder.RenameIndex(
                name: "IX_PlantDetections_UserId",
                table: "plantDetections",
                newName: "IX_plantDetections_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_UserId",
                table: "notifications",
                newName: "IX_notifications_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_InsectDetections_UserId",
                table: "insectDetections",
                newName: "IX_insectDetections_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_plantDetections",
                table: "plantDetections",
                column: "PlantDetectionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_notifications",
                table: "notifications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_insectDetections",
                table: "insectDetections",
                column: "InsectDetectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_insectDetections_AspNetUsers_UserId",
                table: "insectDetections",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_AspNetUsers_UserId",
                table: "notifications",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_plantDetections_AspNetUsers_UserId",
                table: "plantDetections",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
