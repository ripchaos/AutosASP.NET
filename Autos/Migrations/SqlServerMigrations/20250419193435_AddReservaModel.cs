using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Autos.Migrations.SqlServerMigrations
{
    /// <inheritdoc />
    public partial class AddReservaModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comentarios",
                table: "Reservas",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Reservas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaExpiracion",
                table: "Reservas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "VendedorId",
                table: "Reservas",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_VendedorId",
                table: "Reservas",
                column: "VendedorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_AspNetUsers_VendedorId",
                table: "Reservas",
                column: "VendedorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_AspNetUsers_VendedorId",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_VendedorId",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "Comentarios",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "FechaExpiracion",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "VendedorId",
                table: "Reservas");
        }
    }
}
