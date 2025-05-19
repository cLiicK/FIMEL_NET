using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fimel.Models.Migrations
{
    /// <inheritdoc />
    public partial class IncorporacionCampos5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Citas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaHoraInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHoraFinal = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NombrePaciente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorreoPaciente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoDocumento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumeroDocumento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Vigente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Citas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Config",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionesUsuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    DuracionBloqueHorario = table.Column<TimeSpan>(type: "time", nullable: false),
                    Vigente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesUsuario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesUsuario_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Consultas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Paciente = table.Column<int>(type: "int", nullable: false),
                    MotivoConsulta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Anamnesis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExamenFisico = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Diagnostico = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Indicaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Receta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrdenExamenes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoConsulta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Peso = table.Column<double>(type: "float", nullable: true),
                    Talla = table.Column<double>(type: "float", nullable: true),
                    IMC = table.Column<double>(type: "float", nullable: true),
                    PresionArterial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GrupoRH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Inmunizaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaConsulta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaProximoControl = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstadoNutricional = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsuarioCreacion = table.Column<int>(type: "int", nullable: true),
                    Vigente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consultas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HorariosAtencion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "time", nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Vigente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosAtencion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorariosAtencion_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Citas_UsuarioId",
                table: "Citas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesUsuario_UsuarioId",
                table: "ConfiguracionesUsuario",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_HorariosAtencion_UsuarioId",
                table: "HorariosAtencion",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Citas");

            migrationBuilder.DropTable(
                name: "Config");

            migrationBuilder.DropTable(
                name: "ConfiguracionesUsuario");

            migrationBuilder.DropTable(
                name: "Consultas");

            migrationBuilder.DropTable(
                name: "HorariosAtencion");
        }
    }
}
