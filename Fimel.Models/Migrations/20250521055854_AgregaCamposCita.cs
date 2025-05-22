using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fimel.Models.Migrations
{
    /// <inheritdoc />
    public partial class AgregaCamposCita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nota",
                table: "Citas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Citas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nota",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Citas");
        }
    }
}
