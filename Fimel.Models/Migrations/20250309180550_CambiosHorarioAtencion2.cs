using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fimel.Models.Migrations
{
    /// <inheritdoc />
    public partial class CambiosHorarioAtencion2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Citas_HorariosAtencion_HorarioAtencionId",
                table: "Citas");

            migrationBuilder.DropIndex(
                name: "IX_Citas_HorarioAtencionId",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "Disponible",
                table: "HorariosAtencion");

            migrationBuilder.DropColumn(
                name: "EsRecurrente",
                table: "HorariosAtencion");

            migrationBuilder.DropColumn(
                name: "FechaInicio",
                table: "HorariosAtencion");

            migrationBuilder.DropColumn(
                name: "FechaTermino",
                table: "HorariosAtencion");

            migrationBuilder.DropColumn(
                name: "HorarioAtencionId",
                table: "Citas");

            migrationBuilder.RenameColumn(
                name: "DiasRecurrentes",
                table: "HorariosAtencion",
                newName: "DiaSemana");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "HoraFin",
                table: "HorariosAtencion",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "HoraInicio",
                table: "HorariosAtencion",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaHora",
                table: "Citas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HoraFin",
                table: "HorariosAtencion");

            migrationBuilder.DropColumn(
                name: "HoraInicio",
                table: "HorariosAtencion");

            migrationBuilder.DropColumn(
                name: "FechaHora",
                table: "Citas");

            migrationBuilder.RenameColumn(
                name: "DiaSemana",
                table: "HorariosAtencion",
                newName: "DiasRecurrentes");

            migrationBuilder.AddColumn<bool>(
                name: "Disponible",
                table: "HorariosAtencion",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EsRecurrente",
                table: "HorariosAtencion",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaInicio",
                table: "HorariosAtencion",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaTermino",
                table: "HorariosAtencion",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "HorarioAtencionId",
                table: "Citas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Citas_HorarioAtencionId",
                table: "Citas",
                column: "HorarioAtencionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Citas_HorariosAtencion_HorarioAtencionId",
                table: "Citas",
                column: "HorarioAtencionId",
                principalTable: "HorariosAtencion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
