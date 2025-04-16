using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fimel.Models.Migrations
{
    /// <inheritdoc />
    public partial class CambiosHorarioAtencion1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comentario",
                table: "HorariosAtencion",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DiasRecurrentes",
                table: "HorariosAtencion",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "EsRecurrente",
                table: "HorariosAtencion",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comentario",
                table: "HorariosAtencion");

            migrationBuilder.DropColumn(
                name: "DiasRecurrentes",
                table: "HorariosAtencion");

            migrationBuilder.DropColumn(
                name: "EsRecurrente",
                table: "HorariosAtencion");
        }
    }
}
