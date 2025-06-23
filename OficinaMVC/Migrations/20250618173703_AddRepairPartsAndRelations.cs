using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OficinaMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddRepairPartsAndRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Repairs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "AppointmentId",
                table: "Repairs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepairId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RepairParts",
                columns: table => new
                {
                    RepairId = table.Column<int>(type: "int", nullable: false),
                    PartId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairParts", x => new { x.RepairId, x.PartId });
                    table.ForeignKey(
                        name: "FK_RepairParts_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepairParts_Repairs_RepairId",
                        column: x => x.RepairId,
                        principalTable: "Repairs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Repairs_AppointmentId",
                table: "Repairs",
                column: "AppointmentId",
                unique: true,
                filter: "[AppointmentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RepairParts_PartId",
                table: "RepairParts",
                column: "PartId");

            migrationBuilder.AddForeignKey(
                name: "FK_Repairs_Appointments_AppointmentId",
                table: "Repairs",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Repairs_Appointments_AppointmentId",
                table: "Repairs");

            migrationBuilder.DropTable(
                name: "RepairParts");

            migrationBuilder.DropIndex(
                name: "IX_Repairs_AppointmentId",
                table: "Repairs");

            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "Repairs");

            migrationBuilder.DropColumn(
                name: "RepairId",
                table: "Appointments");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Repairs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }
    }
}
