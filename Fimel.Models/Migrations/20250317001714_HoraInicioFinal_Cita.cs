using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fimel.Models.Migrations
{
    /// <inheritdoc />
    public partial class HoraInicioFinal_Cita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FechaHora",
                table: "Citas",
                newName: "FechaHoraInicio");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaHoraFinal",
                table: "Citas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaHoraFinal",
                table: "Citas");

            migrationBuilder.RenameColumn(
                name: "FechaHoraInicio",
                table: "Citas",
                newName: "FechaHora");
        }
    }
}
