using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fimel.Models.Migrations
{
    /// <inheritdoc />
    public partial class UsuarioAPlantillas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "PlantillasConsulta",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasConsulta_UsuarioId",
                table: "PlantillasConsulta",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlantillasConsulta_Usuarios_UsuarioId",
                table: "PlantillasConsulta",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlantillasConsulta_Usuarios_UsuarioId",
                table: "PlantillasConsulta");

            migrationBuilder.DropIndex(
                name: "IX_PlantillasConsulta_UsuarioId",
                table: "PlantillasConsulta");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "PlantillasConsulta");
        }
    }
}
