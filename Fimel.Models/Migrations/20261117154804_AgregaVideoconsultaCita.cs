using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fimel.Models.Migrations
{
    /// <inheritdoc />
    public partial class AgregaVideoconsultaCita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsVideoconsulta",
                table: "Citas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VideoSalaNombre",
                table: "Citas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoToken",
                table: "Citas",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoUrlPaciente",
                table: "Citas",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoUrlProfesional",
                table: "Citas",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Citas_VideoToken",
                table: "Citas",
                column: "VideoToken",
                unique: true,
                filter: "[VideoToken] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Citas_VideoToken",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "EsVideoconsulta",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "VideoSalaNombre",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "VideoToken",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "VideoUrlPaciente",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "VideoUrlProfesional",
                table: "Citas");
        }
    }
}
