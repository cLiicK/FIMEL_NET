using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fimel.Models.Migrations
{
    /// <inheritdoc />
    public partial class AgregaSuscripcionesFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PagosSuscripcion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSuscripcion = table.Column<int>(type: "int", nullable: false),
                    FlowOrder = table.Column<long>(type: "bigint", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Monto = table.Column<int>(type: "int", nullable: false),
                    EstadoFlow = table.Column<int>(type: "int", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RespuestaJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Vigente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosSuscripcion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanesSuscripcion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoFlow = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Monto = table.Column<int>(type: "int", nullable: false),
                    DiasPrueba = table.Column<int>(type: "int", nullable: false),
                    Vigente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanesSuscripcion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SuscripcionesUsuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    IdPlan = table.Column<int>(type: "int", nullable: false),
                    FlowCustomerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FlowSubscriptionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinPrueba = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PagadoHasta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PruebaUsada = table.Column<bool>(type: "bit", nullable: false),
                    TipoTarjeta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ultimos4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Vigente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuscripcionesUsuario", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PagosSuscripcion_FlowOrder",
                table: "PagosSuscripcion",
                column: "FlowOrder",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanesSuscripcion_CodigoFlow",
                table: "PlanesSuscripcion",
                column: "CodigoFlow",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesUsuario_FlowCustomerId",
                table: "SuscripcionesUsuario",
                column: "FlowCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesUsuario_IdUsuario",
                table: "SuscripcionesUsuario",
                column: "IdUsuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PagosSuscripcion");

            migrationBuilder.DropTable(
                name: "PlanesSuscripcion");

            migrationBuilder.DropTable(
                name: "SuscripcionesUsuario");
        }
    }
}
