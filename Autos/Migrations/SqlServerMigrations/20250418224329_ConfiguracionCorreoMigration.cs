using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Autos.Migrations.SqlServerMigrations
{
    /// <inheritdoc />
    public partial class ConfiguracionCorreoMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracionCorreo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Servidor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Puerto = table.Column<int>(type: "int", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiereSSL = table.Column<bool>(type: "bit", nullable: false),
                    EmailRemitente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreRemitente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NotificarReservas = table.Column<bool>(type: "bit", nullable: false),
                    NotificarVentas = table.Column<bool>(type: "bit", nullable: false),
                    NotificarDescuentos = table.Column<bool>(type: "bit", nullable: false),
                    NotificarNuevasReservas = table.Column<bool>(type: "bit", nullable: false),
                    NotificarNuevasVentas = table.Column<bool>(type: "bit", nullable: false),
                    NotificarSolicitudesDescuento = table.Column<bool>(type: "bit", nullable: false),
                    EmailsNotificacionesInternas = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionCorreo", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionCorreo");
        }
    }
}
