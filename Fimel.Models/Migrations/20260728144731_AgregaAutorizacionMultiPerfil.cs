using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fimel.Models.Migrations
{
    /// <inheritdoc />
    public partial class AgregaAutorizacionMultiPerfil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Modulos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Controller = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Vigente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modulos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioPerfil",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    PerfilId = table.Column<int>(type: "int", nullable: false),
                    Vigente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioPerfil", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioPerfil_Perfiles_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioPerfil_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModuloPerfil",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuloId = table.Column<int>(type: "int", nullable: false),
                    PerfilId = table.Column<int>(type: "int", nullable: false),
                    Vigente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuloPerfil", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModuloPerfil_Modulos_ModuloId",
                        column: x => x.ModuloId,
                        principalTable: "Modulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModuloPerfil_Perfiles_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModuloPerfil_ModuloId",
                table: "ModuloPerfil",
                column: "ModuloId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuloPerfil_PerfilId",
                table: "ModuloPerfil",
                column: "PerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioPerfil_PerfilId",
                table: "UsuarioPerfil",
                column: "PerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioPerfil_UsuarioId",
                table: "UsuarioPerfil",
                column: "UsuarioId");

            // Seed: perfil SuperAdmin (Perfiles está excluida de migraciones EF, se administra manual)
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM Perfiles WHERE Id = 4)
BEGIN
    SET IDENTITY_INSERT Perfiles ON;
    INSERT INTO Perfiles (Id, Descripcion, Vigente, FechaCreacion) VALUES (4, N'SuperAdmin', 'S', GETDATE());
    SET IDENTITY_INSERT Perfiles OFF;
END
");

            // Backfill: todo usuario existente conserva el mismo perfil que ya tenía
            migrationBuilder.Sql(@"
INSERT INTO UsuarioPerfil (UsuarioId, PerfilId, Vigente, FechaCreacion)
SELECT Id, IdPerfil, 'S', GETDATE() FROM Usuarios WHERE Vigente = 'S';
");

            // Seed: módulos iniciales (solo los dos que este cambio introduce/modifica)
            migrationBuilder.Sql(@"
SET IDENTITY_INSERT Modulos ON;
INSERT INTO Modulos (Id, Nombre, Controller, Accion, Orden, Vigente, FechaCreacion) VALUES
(1, N'Nueva Consulta', N'Consulta', N'NuevaConsulta', 1, 'S', GETDATE()),
(2, N'Administración', N'Administracion', N'Index', 99, 'S', GETDATE());
SET IDENTITY_INSERT Modulos OFF;

SET IDENTITY_INSERT ModuloPerfil ON;
INSERT INTO ModuloPerfil (Id, ModuloId, PerfilId, Vigente, FechaCreacion) VALUES
(1, 1, 1, 'S', GETDATE()), -- Nueva Consulta / Administrador
(2, 1, 2, 'S', GETDATE()), -- Nueva Consulta / Especialista
(3, 2, 4, 'S', GETDATE()); -- Administración / SuperAdmin
SET IDENTITY_INSERT ModuloPerfil OFF;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Perfiles WHERE Id = 4 AND Descripcion = N'SuperAdmin';");

            migrationBuilder.DropTable(
                name: "ModuloPerfil");

            migrationBuilder.DropTable(
                name: "UsuarioPerfil");

            migrationBuilder.DropTable(
                name: "Modulos");
        }
    }
}
