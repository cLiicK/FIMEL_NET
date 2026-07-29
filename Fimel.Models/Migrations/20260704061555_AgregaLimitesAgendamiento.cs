using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fimel.Models.Migrations
{
    /// <inheritdoc />
    public partial class AgregaLimitesAgendamiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxAntDias",
                table: "ConfiguracionesUsuario",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinAntHoras",
                table: "ConfiguracionesUsuario",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxAntDias",
                table: "ConfiguracionesUsuario");

            migrationBuilder.DropColumn(
                name: "MinAntHoras",
                table: "ConfiguracionesUsuario");
        }
    }
}
