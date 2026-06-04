using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fotografia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogPhase6D : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bitacora",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UsuarioEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Rol = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Accion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    EntidadTipo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    EntidadId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Ip = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    RequestPath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    HttpMethod = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    MetadataJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Severidad = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FechaUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bitacora", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bitacora_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bitacora_Accion_FechaUtc",
                table: "Bitacora",
                columns: new[] { "Accion", "FechaUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Bitacora_CorrelationId",
                table: "Bitacora",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_Bitacora_EntidadTipo_EntidadId_FechaUtc",
                table: "Bitacora",
                columns: new[] { "EntidadTipo", "EntidadId", "FechaUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Bitacora_FechaUtc",
                table: "Bitacora",
                column: "FechaUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Bitacora_Severidad_FechaUtc",
                table: "Bitacora",
                columns: new[] { "Severidad", "FechaUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Bitacora_UsuarioEmail_FechaUtc",
                table: "Bitacora",
                columns: new[] { "UsuarioEmail", "FechaUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Bitacora_UsuarioId_FechaUtc",
                table: "Bitacora",
                columns: new[] { "UsuarioId", "FechaUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bitacora");
        }
    }
}
