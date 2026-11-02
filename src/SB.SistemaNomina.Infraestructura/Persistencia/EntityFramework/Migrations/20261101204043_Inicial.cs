using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SB.SistemaNomina.Infraestructura.Persistencia.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departamentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departamentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntidadesGubernamentales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PoderDelEstado = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Sector = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EstaActiva = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntidadesGubernamentales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Nominas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaInicioSemana = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nominas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreUsuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HashContrasena = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Empleados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrimerNombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApellidoPaterno = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cedula = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    DepartamentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TipoEmpleado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SalarioSemanal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    VentasBrutas = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TarifaComision = table.Column<decimal>(type: "decimal(7,6)", nullable: true),
                    SalarioBase = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SueldoPorHora = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HorasTrabajadas = table.Column<decimal>(type: "decimal(6,3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empleados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Empleados_Departamentos_DepartamentoId",
                        column: x => x.DepartamentoId,
                        principalTable: "Departamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesNomina",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NominaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpleadoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreCompletoEmpleado = table.Column<string>(type: "nvarchar(201)", maxLength: 201, nullable: false),
                    TipoEmpleado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NombreDepartamento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PagoBruto = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesNomina", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesNomina_Nominas_NominaId",
                        column: x => x.NominaId,
                        principalTable: "Nominas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LineasDesgloseNomina",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Concepto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Formula = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DetalleNominaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineasDesgloseNomina", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LineasDesgloseNomina_DetallesNomina_DetalleNominaId",
                        column: x => x.DetalleNominaId,
                        principalTable: "DetallesNomina",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Departamentos_Nombre",
                table: "Departamentos",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesNomina_NominaId",
                table: "DetallesNomina",
                column: "NominaId");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_Cedula",
                table: "Empleados",
                column: "Cedula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_DepartamentoId",
                table: "Empleados",
                column: "DepartamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_Estado_DepartamentoId",
                table: "Empleados",
                columns: new[] { "Estado", "DepartamentoId" });

            migrationBuilder.CreateIndex(
                name: "IX_EntidadesGubernamentales_Nombre",
                table: "EntidadesGubernamentales",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LineasDesgloseNomina_DetalleNominaId",
                table: "LineasDesgloseNomina",
                column: "DetalleNominaId");

            migrationBuilder.CreateIndex(
                name: "IX_Nominas_FechaInicioSemana",
                table: "Nominas",
                column: "FechaInicioSemana",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_NombreUsuario",
                table: "Usuarios",
                column: "NombreUsuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Empleados");

            migrationBuilder.DropTable(
                name: "EntidadesGubernamentales");

            migrationBuilder.DropTable(
                name: "LineasDesgloseNomina");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Departamentos");

            migrationBuilder.DropTable(
                name: "DetallesNomina");

            migrationBuilder.DropTable(
                name: "Nominas");
        }
    }
}
