using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fimel.Models.Migrations
{
    /// <inheritdoc />
    public partial class AgregaSincronizacionSuscripciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FlowOrder",
                table: "PagosSuscripcion",
                newName: "FlowInvoiceId");

            migrationBuilder.RenameIndex(
                name: "IX_PagosSuscripcion_FlowOrder",
                table: "PagosSuscripcion",
                newName: "IX_PagosSuscripcion_FlowInvoiceId");

            migrationBuilder.AddColumn<bool>(
                name: "CancelaAlFinPeriodo",
                table: "SuscripcionesUsuario",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaSincronizacion",
                table: "SuscripcionesUsuario",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "PagosSuscripcion",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodoDesde",
                table: "PagosSuscripcion",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodoHasta",
                table: "PagosSuscripcion",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelaAlFinPeriodo",
                table: "SuscripcionesUsuario");

            migrationBuilder.DropColumn(
                name: "UltimaSincronizacion",
                table: "SuscripcionesUsuario");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "PagosSuscripcion");

            migrationBuilder.DropColumn(
                name: "PeriodoDesde",
                table: "PagosSuscripcion");

            migrationBuilder.DropColumn(
                name: "PeriodoHasta",
                table: "PagosSuscripcion");

            migrationBuilder.RenameColumn(
                name: "FlowInvoiceId",
                table: "PagosSuscripcion",
                newName: "FlowOrder");

            migrationBuilder.RenameIndex(
                name: "IX_PagosSuscripcion_FlowInvoiceId",
                table: "PagosSuscripcion",
                newName: "IX_PagosSuscripcion_FlowOrder");
        }
    }
}
