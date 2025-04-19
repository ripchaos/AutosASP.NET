using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Autos.Migrations.SqlServerMigrations
{
    /// <inheritdoc />
    public partial class SimplificacionModeloSucursal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activa",
                table: "Sucursales",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "GerenteId",
                table: "Sucursales",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ubicacion",
                table: "Sucursales",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_GerenteId",
                table: "Sucursales",
                column: "GerenteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sucursales_AspNetUsers_GerenteId",
                table: "Sucursales",
                column: "GerenteId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sucursales_AspNetUsers_GerenteId",
                table: "Sucursales");

            migrationBuilder.DropIndex(
                name: "IX_Sucursales_GerenteId",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "Activa",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "GerenteId",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "Ubicacion",
                table: "Sucursales");
        }
    }
}
