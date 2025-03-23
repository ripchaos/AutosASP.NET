using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Autos.Migrations
{
    /// <inheritdoc />
    public partial class AjustarNombreDescuentoConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Descuentos",
                table: "Descuentos");

            migrationBuilder.RenameTable(
                name: "Descuentos",
                newName: "DescuentosConfig");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DescuentosConfig",
                table: "DescuentosConfig",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DescuentosConfig",
                table: "DescuentosConfig");

            migrationBuilder.RenameTable(
                name: "DescuentosConfig",
                newName: "Descuentos");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Descuentos",
                table: "Descuentos",
                column: "Id");
        }
    }
}
