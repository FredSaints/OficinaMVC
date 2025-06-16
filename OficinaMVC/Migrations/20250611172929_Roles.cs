using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OficinaMVC.Migrations
{
    /// <inheritdoc />
    public partial class Roles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Vehicles_VehicleId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Repairs_Vehicles_VehicleId",
                table: "Repairs");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_AspNetUsers_OwnerId",
                table: "Vehicles");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Vehicles_VehicleId",
                table: "Appointments",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Repairs_Vehicles_VehicleId",
                table: "Repairs",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_AspNetUsers_OwnerId",
                table: "Vehicles",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Vehicles_VehicleId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Repairs_Vehicles_VehicleId",
                table: "Repairs");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_AspNetUsers_OwnerId",
                table: "Vehicles");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Vehicles_VehicleId",
                table: "Appointments",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Repairs_Vehicles_VehicleId",
                table: "Repairs",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_AspNetUsers_OwnerId",
                table: "Vehicles",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
