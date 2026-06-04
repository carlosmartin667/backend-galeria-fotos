using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fotografia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedSalesPhase5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Destacado",
                table: "ServiciosFotografia",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OrdenDestacado",
                table: "ServiciosFotografia",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CuponCodigo",
                table: "Pedidos",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CuponDescuentoId",
                table: "Pedidos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DescuentoTotal",
                table: "Pedidos",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "Pedidos",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "Destacado",
                table: "PaquetesEvento",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OrdenDestacado",
                table: "PaquetesEvento",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Destacado",
                table: "Fotos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OrdenDestacado",
                table: "Fotos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "CarritosCompra",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "CuponCodigo",
                table: "CarritosCompra",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CuponDescuentoId",
                table: "CarritosCompra",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaUltimoRecordatorioUtc",
                table: "CarritosCompra",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecordatoriosEnviados",
                table: "CarritosCompra",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CarritoAbandonadoRegistros",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CarritoCompraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaDetectadoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaUltimaNotificacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotificacionesEnviadas = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarritoAbandonadoRegistros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarritoAbandonadoRegistros_CarritosCompra_CarritoCompraId",
                        column: x => x.CarritoCompraId,
                        principalTable: "CarritosCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CarritoAbandonadoRegistros_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CarritoAbandonadoRegistros_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CuponesDescuento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TipoDescuento = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ValorDescuento = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoMinimoCompra = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MontoMaximoDescuento = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    FechaInicioUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsosMaximos = table.Column<int>(type: "int", nullable: true),
                    UsosActuales = table.Column<int>(type: "int", nullable: false),
                    UsosMaximosPorUsuario = table.Column<int>(type: "int", nullable: true),
                    SoloPrimerCompra = table.Column<bool>(type: "bit", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuponesDescuento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Testimonios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreCliente = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    EmailCliente = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Texto = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Calificacion = table.Column<int>(type: "int", nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Publicado = table.Column<bool>(type: "bit", nullable: false),
                    Destacado = table.Column<bool>(type: "bit", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PedidoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ServicioFotografiaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EventoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Testimonios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Testimonios_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Testimonios_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Testimonios_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Testimonios_ServiciosFotografia_ServicioFotografiaId",
                        column: x => x.ServicioFotografiaId,
                        principalTable: "ServiciosFotografia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CuponUsos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuponDescuentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PedidoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MontoDescuento = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaUsoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Confirmado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuponUsos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuponUsos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CuponUsos_CuponesDescuento_CuponDescuentoId",
                        column: x => x.CuponDescuentoId,
                        principalTable: "CuponesDescuento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuponUsos_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuponUsos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Promociones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1200)", maxLength: 1200, nullable: true),
                    ImagenUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FechaInicioUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Destacada = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    CuponDescuentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ServicioFotografiaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EventoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promociones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Promociones_CuponesDescuento_CuponDescuentoId",
                        column: x => x.CuponDescuentoId,
                        principalTable: "CuponesDescuento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Promociones_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Promociones_ServiciosFotografia_ServicioFotografiaId",
                        column: x => x.ServicioFotografiaId,
                        principalTable: "ServiciosFotografia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_CuponDescuentoId",
                table: "Pedidos",
                column: "CuponDescuentoId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritosCompra_CuponDescuentoId",
                table: "CarritosCompra",
                column: "CuponDescuentoId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritoAbandonadoRegistros_CarritoCompraId",
                table: "CarritoAbandonadoRegistros",
                column: "CarritoCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritoAbandonadoRegistros_ClienteId",
                table: "CarritoAbandonadoRegistros",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritoAbandonadoRegistros_Estado_FechaDetectadoUtc",
                table: "CarritoAbandonadoRegistros",
                columns: new[] { "Estado", "FechaDetectadoUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CarritoAbandonadoRegistros_UsuarioId",
                table: "CarritoAbandonadoRegistros",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CuponesDescuento_Activo_FechaFinUtc",
                table: "CuponesDescuento",
                columns: new[] { "Activo", "FechaFinUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CuponesDescuento_Codigo",
                table: "CuponesDescuento",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CuponUsos_ClienteId",
                table: "CuponUsos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_CuponUsos_CuponDescuentoId_ClienteId",
                table: "CuponUsos",
                columns: new[] { "CuponDescuentoId", "ClienteId" });

            migrationBuilder.CreateIndex(
                name: "IX_CuponUsos_PedidoId",
                table: "CuponUsos",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_CuponUsos_UsuarioId",
                table: "CuponUsos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Promociones_Activa_Destacada_Orden",
                table: "Promociones",
                columns: new[] { "Activa", "Destacada", "Orden" });

            migrationBuilder.CreateIndex(
                name: "IX_Promociones_CuponDescuentoId",
                table: "Promociones",
                column: "CuponDescuentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Promociones_EventoId",
                table: "Promociones",
                column: "EventoId");

            migrationBuilder.CreateIndex(
                name: "IX_Promociones_ServicioFotografiaId",
                table: "Promociones",
                column: "ServicioFotografiaId");

            migrationBuilder.CreateIndex(
                name: "IX_Testimonios_ClienteId",
                table: "Testimonios",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Testimonios_EventoId",
                table: "Testimonios",
                column: "EventoId");

            migrationBuilder.CreateIndex(
                name: "IX_Testimonios_PedidoId",
                table: "Testimonios",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Testimonios_Publicado_Destacado",
                table: "Testimonios",
                columns: new[] { "Publicado", "Destacado" });

            migrationBuilder.CreateIndex(
                name: "IX_Testimonios_ServicioFotografiaId",
                table: "Testimonios",
                column: "ServicioFotografiaId");

            migrationBuilder.AddForeignKey(
                name: "FK_CarritosCompra_CuponesDescuento_CuponDescuentoId",
                table: "CarritosCompra",
                column: "CuponDescuentoId",
                principalTable: "CuponesDescuento",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_CuponesDescuento_CuponDescuentoId",
                table: "Pedidos",
                column: "CuponDescuentoId",
                principalTable: "CuponesDescuento",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarritosCompra_CuponesDescuento_CuponDescuentoId",
                table: "CarritosCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_CuponesDescuento_CuponDescuentoId",
                table: "Pedidos");

            migrationBuilder.DropTable(
                name: "CarritoAbandonadoRegistros");

            migrationBuilder.DropTable(
                name: "CuponUsos");

            migrationBuilder.DropTable(
                name: "Promociones");

            migrationBuilder.DropTable(
                name: "Testimonios");

            migrationBuilder.DropTable(
                name: "CuponesDescuento");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_CuponDescuentoId",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_CarritosCompra_CuponDescuentoId",
                table: "CarritosCompra");

            migrationBuilder.DropColumn(
                name: "Destacado",
                table: "ServiciosFotografia");

            migrationBuilder.DropColumn(
                name: "OrdenDestacado",
                table: "ServiciosFotografia");

            migrationBuilder.DropColumn(
                name: "CuponCodigo",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "CuponDescuentoId",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "DescuentoTotal",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "Destacado",
                table: "PaquetesEvento");

            migrationBuilder.DropColumn(
                name: "OrdenDestacado",
                table: "PaquetesEvento");

            migrationBuilder.DropColumn(
                name: "Destacado",
                table: "Fotos");

            migrationBuilder.DropColumn(
                name: "OrdenDestacado",
                table: "Fotos");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "CarritosCompra");

            migrationBuilder.DropColumn(
                name: "CuponCodigo",
                table: "CarritosCompra");

            migrationBuilder.DropColumn(
                name: "CuponDescuentoId",
                table: "CarritosCompra");

            migrationBuilder.DropColumn(
                name: "FechaUltimoRecordatorioUtc",
                table: "CarritosCompra");

            migrationBuilder.DropColumn(
                name: "RecordatoriosEnviados",
                table: "CarritosCompra");
        }
    }
}
