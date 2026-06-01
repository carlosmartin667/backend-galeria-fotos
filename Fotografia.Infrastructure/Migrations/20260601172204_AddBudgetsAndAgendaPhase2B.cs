using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fotografia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetsAndAgendaPhase2B : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitudesPresupuesto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    WhatsApp = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    TipoEvento = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ServicioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaTentativaUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Lugar = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: true),
                    CantidadInvitados = table.Column<int>(type: "int", nullable: true),
                    Mensaje = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValue: "Nuevo"),
                    Activa = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesPresupuesto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesPresupuesto_ServiciosFotografia_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "ServiciosFotografia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AgendaItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValue: "Otro"),
                    FechaInicioUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFinUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValue: "Programado"),
                    EventoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SesionPrivadaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SolicitudPresupuestoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgendaItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgendaItems_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AgendaItems_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AgendaItems_SesionesPrivadas_SesionPrivadaId",
                        column: x => x.SesionPrivadaId,
                        principalTable: "SesionesPrivadas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AgendaItems_SolicitudesPresupuesto_SolicitudPresupuestoId",
                        column: x => x.SolicitudPresupuestoId,
                        principalTable: "SolicitudesPresupuesto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgendaItems_Activo_Estado",
                table: "AgendaItems",
                columns: new[] { "Activo", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_AgendaItems_ClienteId",
                table: "AgendaItems",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_AgendaItems_EventoId",
                table: "AgendaItems",
                column: "EventoId");

            migrationBuilder.CreateIndex(
                name: "IX_AgendaItems_FechaInicioUtc_FechaFinUtc",
                table: "AgendaItems",
                columns: new[] { "FechaInicioUtc", "FechaFinUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AgendaItems_SesionPrivadaId",
                table: "AgendaItems",
                column: "SesionPrivadaId");

            migrationBuilder.CreateIndex(
                name: "IX_AgendaItems_SolicitudPresupuestoId",
                table: "AgendaItems",
                column: "SolicitudPresupuestoId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesPresupuesto_Activa_FechaCreacionUtc",
                table: "SolicitudesPresupuesto",
                columns: new[] { "Activa", "FechaCreacionUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesPresupuesto_Estado_FechaCreacionUtc",
                table: "SolicitudesPresupuesto",
                columns: new[] { "Estado", "FechaCreacionUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesPresupuesto_ServicioId",
                table: "SolicitudesPresupuesto",
                column: "ServicioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgendaItems");

            migrationBuilder.DropTable(
                name: "SolicitudesPresupuesto");
        }
    }
}
