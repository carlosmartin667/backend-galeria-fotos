using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fotografia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDownloadLimitsAndProtection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activa",
                table: "Descargas",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActualizacionUtc",
                table: "Descargas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxDescargas",
                table: "Descargas",
                type: "int",
                nullable: true,
                defaultValue: 5);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaDescargaUtc",
                table: "Descargas",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activa",
                table: "Descargas");

            migrationBuilder.DropColumn(
                name: "FechaActualizacionUtc",
                table: "Descargas");

            migrationBuilder.DropColumn(
                name: "MaxDescargas",
                table: "Descargas");

            migrationBuilder.DropColumn(
                name: "UltimaDescargaUtc",
                table: "Descargas");
        }
    }
}
