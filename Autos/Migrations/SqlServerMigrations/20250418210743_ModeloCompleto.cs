using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Autos.Migrations.SqlServerMigrations
{
    /// <inheritdoc />
    public partial class ModeloCompleto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_AspNetUsers_UsuarioId",
                table: "Ventas");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "Ventas",
                newName: "VendedorId");

            migrationBuilder.RenameColumn(
                name: "FechaVenta",
                table: "Ventas",
                newName: "Fecha");

            migrationBuilder.RenameIndex(
                name: "IX_Ventas_UsuarioId",
                table: "Ventas",
                newName: "IX_Ventas_VendedorId");

            migrationBuilder.AddColumn<string>(
                name: "ClienteId",
                table: "Ventas",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PorcentajeDescuento",
                table: "Ventas",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "SolicitudesDescuento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AutoId = table.Column<int>(type: "int", nullable: false),
                    VendedorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PorcentajeSolicitado = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Justificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GerenteId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Aprobada = table.Column<bool>(type: "bit", nullable: true),
                    FechaResolucion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ComentarioGerente = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesDescuento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesDescuento_AspNetUsers_GerenteId",
                        column: x => x.GerenteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudesDescuento_AspNetUsers_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudesDescuento_Autos_AutoId",
                        column: x => x.AutoId,
                        principalTable: "Autos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_ClienteId",
                table: "Ventas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesDescuento_AutoId",
                table: "SolicitudesDescuento",
                column: "AutoId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesDescuento_GerenteId",
                table: "SolicitudesDescuento",
                column: "GerenteId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesDescuento_VendedorId",
                table: "SolicitudesDescuento",
                column: "VendedorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_AspNetUsers_ClienteId",
                table: "Ventas",
                column: "ClienteId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_AspNetUsers_VendedorId",
                table: "Ventas",
                column: "VendedorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_AspNetUsers_ClienteId",
                table: "Ventas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_AspNetUsers_VendedorId",
                table: "Ventas");

            migrationBuilder.DropTable(
                name: "SolicitudesDescuento");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_ClienteId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "PorcentajeDescuento",
                table: "Ventas");

            migrationBuilder.RenameColumn(
                name: "VendedorId",
                table: "Ventas",
                newName: "UsuarioId");

            migrationBuilder.RenameColumn(
                name: "Fecha",
                table: "Ventas",
                newName: "FechaVenta");

            migrationBuilder.RenameIndex(
                name: "IX_Ventas_VendedorId",
                table: "Ventas",
                newName: "IX_Ventas_UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_AspNetUsers_UsuarioId",
                table: "Ventas",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
