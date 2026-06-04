using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fotografia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalManagementPhase3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotasInternas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntidadTipo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    EntidadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasInternas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotasInternas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PedidoEstadoHistorial",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PedidoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstadoAnterior = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    EstadoNuevo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaCambioUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidoEstadoHistorial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidoEstadoHistorial_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PedidoEstadoHistorial_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotasInternas_Activa_FechaCreacionUtc",
                table: "NotasInternas",
                columns: new[] { "Activa", "FechaCreacionUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_NotasInternas_EntidadTipo_EntidadId",
                table: "NotasInternas",
                columns: new[] { "EntidadTipo", "EntidadId" });

            migrationBuilder.CreateIndex(
                name: "IX_NotasInternas_UsuarioId",
                table: "NotasInternas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoEstadoHistorial_PedidoId_FechaCambioUtc",
                table: "PedidoEstadoHistorial",
                columns: new[] { "PedidoId", "FechaCambioUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PedidoEstadoHistorial_UsuarioId",
                table: "PedidoEstadoHistorial",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotasInternas");

            migrationBuilder.DropTable(
                name: "PedidoEstadoHistorial");
        }
    }
}
