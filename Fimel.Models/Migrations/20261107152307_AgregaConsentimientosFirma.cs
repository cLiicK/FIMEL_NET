using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fimel.Models.Migrations
{
    /// <inheritdoc />
    public partial class AgregaConsentimientosFirma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsentimientosFirma",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TipoConsentimiento = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    TituloDocumento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdPaciente = table.Column<int>(type: "int", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    IdInstitucion = table.Column<int>(type: "int", nullable: true),
                    NombrePaciente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RutPaciente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombreProfesional = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RutProfesional = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TituloProfesional = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirmaProfesional = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombreInstitucion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HtmlCuerpo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFirma = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TipoFirmante = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NombreFirmante = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RutFirmante = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirmaPaciente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpFirma = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgentFirma = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HashDocumento = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Vigente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentimientosFirma", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsentimientosFirma_IdPaciente",
                table: "ConsentimientosFirma",
                column: "IdPaciente");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentimientosFirma_Token",
                table: "ConsentimientosFirma",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsentimientosFirma");
        }
    }
}
