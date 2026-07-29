using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fimel.Models.Migrations
{
    /// <inheritdoc />
    public partial class AgregaTipoConsultaCatalogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipoConsultaId",
                table: "Consultas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TipoConsulta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Vigente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoConsulta", x => x.Id);
                });

            migrationBuilder.Sql(@"
SET IDENTITY_INSERT TipoConsulta ON;
INSERT INTO TipoConsulta (Id, Nombre, Orden, Vigente, FechaCreacion) VALUES
(1,  N'Regulación Fecundidad',    1,  'S', GETDATE()),
(2,  N'Control Ginecológico',     2,  'S', GETDATE()),
(3,  N'Consulta Ginecológica',    3,  'S', GETDATE()),
(4,  N'Control Adolescente',      4,  'S', GETDATE()),
(5,  N'Control Prenatal',         5,  'S', GETDATE()),
(6,  N'Lactancia Materna',        6,  'S', GETDATE()),
(7,  N'Menopausia y Climaterio',  7,  'S', GETDATE()),
(8,  N'Otra Consejería',          8,  'S', GETDATE()),
(9,  N'Ecografía',                9,  'S', GETDATE()),
(10, N'HIFU',                     10, 'S', GETDATE()),
(11, N'Piso Pélvico',             11, 'S', GETDATE()),
(12, N'Reproducción asistida',    12, 'S', GETDATE()),
(13, N'Otros',                    13, 'S', GETDATE());
SET IDENTITY_INSERT TipoConsulta OFF;
");

            // Backfill: consultas existentes se emparejan con el catálogo por coincidencia exacta
            // del texto legado. El texto de Consultas.TipoConsulta NO se toca ni se borra.
            migrationBuilder.Sql(@"
UPDATE c SET c.TipoConsultaId = m.NuevoId
FROM Consultas c
JOIN (VALUES
    (N'Regulacion Fecundidad',    1),
    (N'Control Ginecologico',     2),
    (N'Consulta Ginecologica',    3),
    (N'Control Adolescente',      4),
    (N'Control Prenatal',         5),
    (N'Lactancia Materna',        6),
    (N'Menopausia y Climaterio',  7),
    (N'Otra Consejeria',          8),
    (N'Ecografia',                9),
    (N'HIFU',                     10),
    (N'Piso Pélvico',             11),
    (N'Reproducción asistida',    12),
    (N'Otros',                    13)
) AS m(ValorLegado, NuevoId) ON c.TipoConsulta = m.ValorLegado
WHERE c.TipoConsultaId IS NULL;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TipoConsulta");

            migrationBuilder.DropColumn(
                name: "TipoConsultaId",
                table: "Consultas");
        }
    }
}
