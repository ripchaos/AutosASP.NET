using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Autos.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregadoCombustibleAuto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Combustible",
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
                name: "Combustible",
                table: "Autos");
        }
    }
}
