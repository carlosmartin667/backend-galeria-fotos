using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fotografia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationsAutomationPhase4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notificaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Canal = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    DestinatarioEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntidadTipo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    EntidadId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CorrelationKey = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: true),
                    Intentos = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxIntentos = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    Error = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Leida = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaLecturaUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProgramadaParaUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnviadaEnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notificaciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PlantillasNotificacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Canal = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Asunto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CuerpoHtml = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CuerpoTexto = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillasNotificacion", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_Canal",
                table: "Notificaciones",
                column: "Canal");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_CorrelationKey",
                table: "Notificaciones",
                column: "CorrelationKey");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_EntidadTipo_EntidadId",
                table: "Notificaciones",
                columns: new[] { "EntidadTipo", "EntidadId" });

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_Estado",
                table: "Notificaciones",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_UsuarioId",
                table: "Notificaciones",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasNotificacion_Activa",
                table: "PlantillasNotificacion",
                column: "Activa");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasNotificacion_Codigo",
                table: "PlantillasNotificacion",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notificaciones");

            migrationBuilder.DropTable(
                name: "PlantillasNotificacion");
        }
    }
}
