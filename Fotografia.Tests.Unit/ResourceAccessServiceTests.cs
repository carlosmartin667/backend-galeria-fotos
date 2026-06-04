using FluentAssertions;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Fotografia.Infrastructure.Services;
using NSubstitute;

namespace Fotografia.Tests.Unit;

public sealed class ResourceAccessServiceTests
{
    [Fact]
    public async Task Guest_can_access_active_public_published_event()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var eventoId = Guid.NewGuid();
        await SeedEventoAsync(dbContext, CreateEvento(eventoId, EventoVisibilidades.Publico, EventoEstados.Publicado, true));
        var service = CreateService(dbContext, isAuthenticated: false);

        var canAccess = await service.CanAccessEventoAsync(eventoId);

        canAccess.Should().BeTrue();
    }

    [Fact]
    public async Task User_cannot_access_foreign_private_event()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var eventoId = Guid.NewGuid();
        await SeedEventoAsync(dbContext, CreateEvento(eventoId, EventoVisibilidades.Privado, EventoEstados.Publicado, true, Guid.NewGuid()));
        var service = CreateService(dbContext, isAuthenticated: true, userId: Guid.NewGuid());

        var canAccess = await service.CanAccessEventoAsync(eventoId);

        canAccess.Should().BeFalse();
    }

    [Fact]
    public async Task Admin_can_access_existing_event()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var eventoId = Guid.NewGuid();
        await SeedEventoAsync(dbContext, CreateEvento(eventoId, EventoVisibilidades.Oculto, EventoEstados.Borrador, false));
        var service = CreateService(dbContext, isAuthenticated: true, isAdmin: true, userId: Guid.NewGuid());

        var canAccess = await service.CanAccessEventoAsync(eventoId);

        canAccess.Should().BeTrue();
    }

    private static ResourceAccessService CreateService(
        AppDbContext dbContext,
        bool isAuthenticated,
        bool isAdmin = false,
        Guid? userId = null)
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.IsAuthenticated.Returns(isAuthenticated);
        currentUser.IsAdmin.Returns(isAdmin);
        currentUser.UserId.Returns(userId);

        return new ResourceAccessService(dbContext, currentUser);
    }

    private static Evento CreateEvento(
        Guid id,
        string visibilidad,
        string estado,
        bool activo,
        Guid? creadoPorUsuarioId = null)
    {
        return new Evento
        {
            Id = id,
            Nombre = $"Evento {id:N}",
            Slug = $"evento-{id:N}",
            FechaEventoUtc = DateTime.UtcNow.AddDays(7),
            Visibilidad = visibilidad,
            Estado = estado,
            Activo = activo,
            CreadoPorUsuarioId = creadoPorUsuarioId
        };
    }

    private static async Task SeedEventoAsync(AppDbContext dbContext, Evento evento)
    {
        dbContext.Eventos.Add(evento);
        await dbContext.SaveChangesAsync();
    }
}
