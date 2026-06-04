using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fotografia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventVisibilityCoverAndPhotoBulk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fotos_Eventos_EventoId",
                table: "Fotos");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActualizacionUtc",
                table: "Fotos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Procesada",
                table: "Fotos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TieneMarcaAgua",
                table: "Fotos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Eventos",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "Publicado",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Eventos",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaLimiteCompraUtc",
                table: "Eventos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PortadaFotoId",
                table: "Eventos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Visibilidad",
                table: "Eventos",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "Publico");

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_Activo_Estado_Visibilidad",
                table: "Eventos",
                columns: new[] { "Activo", "Estado", "Visibilidad" });

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_PortadaFotoId",
                table: "Eventos",
                column: "PortadaFotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Eventos_Fotos_PortadaFotoId",
                table: "Eventos",
                column: "PortadaFotoId",
                principalTable: "Fotos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Fotos_Eventos_EventoId",
                table: "Fotos",
                column: "EventoId",
                principalTable: "Eventos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Eventos_Fotos_PortadaFotoId",
                table: "Eventos");

            migrationBuilder.DropForeignKey(
                name: "FK_Fotos_Eventos_EventoId",
                table: "Fotos");

            migrationBuilder.DropIndex(
                name: "IX_Eventos_Activo_Estado_Visibilidad",
                table: "Eventos");

            migrationBuilder.DropIndex(
                name: "IX_Eventos_PortadaFotoId",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "FechaActualizacionUtc",
                table: "Fotos");

            migrationBuilder.DropColumn(
                name: "Procesada",
                table: "Fotos");

            migrationBuilder.DropColumn(
                name: "TieneMarcaAgua",
                table: "Fotos");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "FechaLimiteCompraUtc",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "PortadaFotoId",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "Visibilidad",
                table: "Eventos");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Eventos",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldDefaultValue: "Publicado");

            migrationBuilder.AddForeignKey(
                name: "FK_Fotos_Eventos_EventoId",
                table: "Fotos",
                column: "EventoId",
                principalTable: "Eventos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
