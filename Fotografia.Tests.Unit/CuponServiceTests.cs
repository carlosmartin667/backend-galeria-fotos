using FluentAssertions;
using Fotografia.Application.DTOs.Cupones;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Fotografia.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fotografia.Tests.Unit;

public sealed class CuponServiceTests
{
    [Fact]
    public async Task ValidarParaClienteAsync_rejects_expired_coupon()
    {
        await using var dbContext = TestDbContextFactory.Create();
        await SeedCouponAsync(dbContext, new CuponDescuento
        {
            Codigo = "VENCIDO",
            TipoDescuento = CuponTipos.Porcentaje,
            ValorDescuento = 10,
            FechaFinUtc = DateTime.UtcNow.AddDays(-1),
            Activo = true
        });
        var service = CreateService(dbContext);

        var result = await service.ValidarParaClienteAsync("VENCIDO", 100m, Guid.NewGuid(), Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("vencido");
    }

    [Fact]
    public async Task CreateAsync_rejects_invalid_percentage()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var service = CreateService(dbContext, isAdmin: true);

        var result = await service.CreateAsync(new CrearCuponDescuentoRequestDto
        {
            Codigo = "INVALIDO",
            TipoDescuento = CuponTipos.Porcentaje,
            ValorDescuento = 150,
            Activo = true
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("porcentaje");
    }

    [Fact]
    public async Task ValidarParaClienteAsync_caps_discount_to_subtotal()
    {
        await using var dbContext = TestDbContextFactory.Create();
        await SeedCouponAsync(dbContext, new CuponDescuento
        {
            Codigo = "GRANDE",
            TipoDescuento = CuponTipos.MontoFijo,
            ValorDescuento = 500,
            Activo = true
        });
        var service = CreateService(dbContext);

        var result = await service.ValidarParaClienteAsync("GRANDE", 100m, Guid.NewGuid(), Guid.NewGuid());

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Descuento.Should().Be(100m);
        result.Data.TotalFinal.Should().Be(0m);
    }

    [Fact]
    public async Task ValidarParaClienteAsync_rejects_inactive_coupon()
    {
        await using var dbContext = TestDbContextFactory.Create();
        await SeedCouponAsync(dbContext, new CuponDescuento
        {
            Codigo = "INACTIVO",
            TipoDescuento = CuponTipos.Porcentaje,
            ValorDescuento = 10,
            Activo = false
        });
        var service = CreateService(dbContext);

        var result = await service.ValidarParaClienteAsync("INACTIVO", 100m, Guid.NewGuid(), Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("activo");
    }

    private static CuponService CreateService(AppDbContext dbContext, bool isAdmin = false)
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.IsAdmin.Returns(isAdmin);
        currentUser.IsAuthenticated.Returns(isAdmin);
        currentUser.UserId.Returns(isAdmin ? Guid.NewGuid() : null);

        return new CuponService(
            dbContext,
            currentUser,
            Substitute.For<INotificacionService>(),
            Substitute.For<ILogger<CuponService>>());
    }

    private static async Task SeedCouponAsync(AppDbContext dbContext, CuponDescuento cupon)
    {
        dbContext.CuponesDescuento.Add(cupon);
        await dbContext.SaveChangesAsync();
    }
}
