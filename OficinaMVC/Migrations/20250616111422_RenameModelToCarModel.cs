using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OficinaMVC.Migrations
{
    /// <inheritdoc />
    public partial class RenameModelToCarModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Model",
                table: "Vehicles",
                newName: "CarModel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CarModel",
                table: "Vehicles",
                newName: "Model");
        }
    }
}
