using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fotografia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCartPackagesOwnerProfilePrivateSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Eventos_EventoId",
                table: "Pedidos");

            migrationBuilder.AlterColumn<Guid>(
                name: "EventoId",
                table: "Pedidos",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "FotoId",
                table: "Descargas",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "EventoId",
                table: "Descargas",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "FotoPrivadaId",
                table: "Descargas",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CarritosCompra",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarritosCompra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarritosCompra_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaquetesEvento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IncluyeTodasLasFotos = table.Column<bool>(type: "bit", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaquetesEvento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaquetesEvento_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerfilesFotografa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Biografia = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    WhatsApp = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Instagram = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Facebook = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    TikTok = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    SitioWeb = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CorreoPublico = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Ciudad = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Provincia = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Pais = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    FotoPerfilUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BannerUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TextoBienvenida = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilesFotografa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SesionesPrivadas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaSesionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PrecioPaquete = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SesionesPrivadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SesionesPrivadas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FotosPrivadas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SesionPrivadaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(700)", maxLength: 700, nullable: false),
                    PreviewUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MarcaAguaStorageKey = table.Column<string>(type: "nvarchar(700)", maxLength: 700, nullable: true),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FotosPrivadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FotosPrivadas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FotosPrivadas_SesionesPrivadas_SesionPrivadaId",
                        column: x => x.SesionPrivadaId,
                        principalTable: "SesionesPrivadas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarritoItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CarritoCompraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoItem = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FotoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PaqueteEventoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FotoPrivadaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarritoItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarritoItems_CarritosCompra_CarritoCompraId",
                        column: x => x.CarritoCompraId,
                        principalTable: "CarritosCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CarritoItems_FotosPrivadas_FotoPrivadaId",
                        column: x => x.FotoPrivadaId,
                        principalTable: "FotosPrivadas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CarritoItems_Fotos_FotoId",
                        column: x => x.FotoId,
                        principalTable: "Fotos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CarritoItems_PaquetesEvento_PaqueteEventoId",
                        column: x => x.PaqueteEventoId,
                        principalTable: "PaquetesEvento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PedidoItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PedidoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoItem = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FotoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PaqueteEventoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FotoPrivadaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidoItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidoItems_FotosPrivadas_FotoPrivadaId",
                        column: x => x.FotoPrivadaId,
                        principalTable: "FotosPrivadas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PedidoItems_Fotos_FotoId",
                        column: x => x.FotoId,
                        principalTable: "Fotos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PedidoItems_PaquetesEvento_PaqueteEventoId",
                        column: x => x.PaqueteEventoId,
                        principalTable: "PaquetesEvento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PedidoItems_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Descargas_FotoPrivadaId",
                table: "Descargas",
                column: "FotoPrivadaId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritoItems_CarritoCompraId",
                table: "CarritoItems",
                column: "CarritoCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritoItems_CarritoCompraId_TipoItem_FotoId",
                table: "CarritoItems",
                columns: new[] { "CarritoCompraId", "TipoItem", "FotoId" },
                unique: true,
                filter: "[FotoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CarritoItems_CarritoCompraId_TipoItem_FotoPrivadaId",
                table: "CarritoItems",
                columns: new[] { "CarritoCompraId", "TipoItem", "FotoPrivadaId" },
                unique: true,
                filter: "[FotoPrivadaId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CarritoItems_CarritoCompraId_TipoItem_PaqueteEventoId",
                table: "CarritoItems",
                columns: new[] { "CarritoCompraId", "TipoItem", "PaqueteEventoId" },
                unique: true,
                filter: "[PaqueteEventoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CarritoItems_FotoId",
                table: "CarritoItems",
                column: "FotoId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritoItems_FotoPrivadaId",
                table: "CarritoItems",
                column: "FotoPrivadaId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritoItems_PaqueteEventoId",
                table: "CarritoItems",
                column: "PaqueteEventoId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritosCompra_UsuarioId_Estado",
                table: "CarritosCompra",
                columns: new[] { "UsuarioId", "Estado" },
                unique: true,
                filter: "[Estado] = 'Activo'");

            migrationBuilder.CreateIndex(
                name: "IX_FotosPrivadas_ClienteId",
                table: "FotosPrivadas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_FotosPrivadas_SesionPrivadaId",
                table: "FotosPrivadas",
                column: "SesionPrivadaId");

            migrationBuilder.CreateIndex(
                name: "IX_FotosPrivadas_SesionPrivadaId_StorageKey",
                table: "FotosPrivadas",
                columns: new[] { "SesionPrivadaId", "StorageKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaquetesEvento_EventoId",
                table: "PaquetesEvento",
                column: "EventoId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItems_FotoId",
                table: "PedidoItems",
                column: "FotoId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItems_FotoPrivadaId",
                table: "PedidoItems",
                column: "FotoPrivadaId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItems_PaqueteEventoId",
                table: "PedidoItems",
                column: "PaqueteEventoId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItems_PedidoId",
                table: "PedidoItems",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilesFotografa_Activa",
                table: "PerfilesFotografa",
                column: "Activa",
                unique: true,
                filter: "[Activa] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SesionesPrivadas_ClienteId",
                table: "SesionesPrivadas",
                column: "ClienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Descargas_FotosPrivadas_FotoPrivadaId",
                table: "Descargas",
                column: "FotoPrivadaId",
                principalTable: "FotosPrivadas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Eventos_EventoId",
                table: "Pedidos",
                column: "EventoId",
                principalTable: "Eventos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Descargas_FotosPrivadas_FotoPrivadaId",
                table: "Descargas");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Eventos_EventoId",
                table: "Pedidos");

            migrationBuilder.DropTable(
                name: "CarritoItems");

            migrationBuilder.DropTable(
                name: "PedidoItems");

            migrationBuilder.DropTable(
                name: "PerfilesFotografa");

            migrationBuilder.DropTable(
                name: "CarritosCompra");

            migrationBuilder.DropTable(
                name: "FotosPrivadas");

            migrationBuilder.DropTable(
                name: "PaquetesEvento");

            migrationBuilder.DropTable(
                name: "SesionesPrivadas");

            migrationBuilder.DropIndex(
                name: "IX_Descargas_FotoPrivadaId",
                table: "Descargas");

            migrationBuilder.DropColumn(
                name: "FotoPrivadaId",
                table: "Descargas");

            migrationBuilder.AlterColumn<Guid>(
                name: "EventoId",
                table: "Pedidos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "FotoId",
                table: "Descargas",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "EventoId",
                table: "Descargas",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Eventos_EventoId",
                table: "Pedidos",
                column: "EventoId",
                principalTable: "Eventos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
