using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fimel.Models.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposAMano : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Campos nuevos en Consultas
            migrationBuilder.AddColumn<string>(
                name: "PresionArterial",
                table: "Consultas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaConsulta",
                table: "Consultas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaProximoControl",
                table: "Consultas",
                type: "datetime2",
                nullable: true);

            // Campos nuevos en Pacientes
            migrationBuilder.AddColumn<string>(
                name: "GrupoRH",
                table: "Pacientes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Inmunizaciones",
                table: "Pacientes",
                type: "nvarchar(max)",
                nullable: true);
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Campos en Consultas
            migrationBuilder.DropColumn(
                name: "PresionArterial",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "FechaConsulta",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "FechaProximoControl",
                table: "Consultas");

            // Campos en Pacientes
            migrationBuilder.DropColumn(
                name: "GrupoRH",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "Inmunizaciones",
                table: "Pacientes");
        }

    }
}
