using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroTech.Migrations
{
    /// <inheritdoc />
    public partial class FixShadowProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Crops_Farms_FarmId1",
                table: "Crops");

            migrationBuilder.DropForeignKey(
                name: "FK_Sensors_Farms_FarmId1",
                table: "Sensors");

            migrationBuilder.DropIndex(
                name: "IX_Sensors_FarmId1",
                table: "Sensors");

            migrationBuilder.DropIndex(
                name: "IX_Crops_FarmId1",
                table: "Crops");

            migrationBuilder.DropColumn(
                name: "FarmId1",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "FarmId1",
                table: "Crops");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FarmId1",
                table: "Sensors",
                type: "RAW(16)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FarmId1",
                table: "Crops",
                type: "RAW(16)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_FarmId1",
                table: "Sensors",
                column: "FarmId1");

            migrationBuilder.CreateIndex(
                name: "IX_Crops_FarmId1",
                table: "Crops",
                column: "FarmId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Crops_Farms_FarmId1",
                table: "Crops",
                column: "FarmId1",
                principalTable: "Farms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sensors_Farms_FarmId1",
                table: "Sensors",
                column: "FarmId1",
                principalTable: "Farms",
                principalColumn: "Id");
        }
    }
}
