using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Autos.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarKilometrajeYTransmisionAuto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Kilometraje",
                table: "Autos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Transmision",
                table: "Autos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kilometraje",
                table: "Autos");

            migrationBuilder.DropColumn(
                name: "Transmision",
                table: "Autos");
        }
    }
}
