using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Autos.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposReservaYSolicitud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRespuesta",
                table: "SolicitudesDescuento",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoRechazo",
                table: "SolicitudesDescuento",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCancelacion",
                table: "Reservas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaConcretacion",
                table: "Reservas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoCancelacion",
                table: "Reservas",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaRespuesta",
                table: "SolicitudesDescuento");

            migrationBuilder.DropColumn(
                name: "MotivoRechazo",
                table: "SolicitudesDescuento");

            migrationBuilder.DropColumn(
                name: "FechaCancelacion",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "FechaConcretacion",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "MotivoCancelacion",
                table: "Reservas");
        }
    }
}
