using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fimel.Models.Migrations
{
    /// <inheritdoc />
    public partial class Talla : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
           name: "Talla",
           table: "Pacientes",
           type: "int",
           nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
            name: "Talla",
            table: "Pacientes");
        }
    }
}
