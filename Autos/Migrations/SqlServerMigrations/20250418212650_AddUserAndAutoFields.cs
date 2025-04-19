using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Autos.Migrations.SqlServerMigrations
{
    /// <inheritdoc />
    public partial class AddUserAndAutoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PorcentajeDescuento",
                table: "Autos",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "TieneDescuento",
                table: "Autos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AutoInteresadoId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "AspNetUsers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Identificacion",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VendedorAsignadoId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_AutoInteresadoId",
                table: "AspNetUsers",
                column: "AutoInteresadoId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_VendedorAsignadoId",
                table: "AspNetUsers",
                column: "VendedorAsignadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_VendedorAsignadoId",
                table: "AspNetUsers",
                column: "VendedorAsignadoId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Autos_AutoInteresadoId",
                table: "AspNetUsers",
                column: "AutoInteresadoId",
                principalTable: "Autos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_VendedorAsignadoId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Autos_AutoInteresadoId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_AutoInteresadoId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_VendedorAsignadoId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PorcentajeDescuento",
                table: "Autos");

            migrationBuilder.DropColumn(
                name: "TieneDescuento",
                table: "Autos");

            migrationBuilder.DropColumn(
                name: "AutoInteresadoId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Identificacion",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "VendedorAsignadoId",
                table: "AspNetUsers");
        }
    }
}
