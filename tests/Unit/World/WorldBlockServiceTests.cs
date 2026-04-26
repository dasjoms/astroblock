using Astroblock.Core.Coords;
using Astroblock.Core.Events;
using Astroblock.Core.Interfaces;
using Astroblock.World;
using Astroblock.World.Storage;

namespace Astroblock.UnitTests.World;

public sealed class WorldBlockServiceTests
{
    [Theory]
    [InlineData(-1, -1, -1)]
    [InlineData(-32, 0, 31)]
    [InlineData(-33, -64, 32)]
    [InlineData(31, 31, 31)]
    [InlineData(32, 32, 32)]
    [InlineData(0, -32, 63)]
    public void SetBlock_UsesExpectedChunkAndLocalMapping_ForNegativeAndBoundaryCoords(int wx, int wy, int wz)
    {
        var store = new InMemoryChunkStore();
        var service = new WorldBlockService(store);

        service.SetBlock(wx, wy, wz, 7);

        var expectedChunk = CoordConverter.WorldToChunk(wx, wy, wz);
        var expectedLocal = CoordConverter.WorldToLocal(wx, wy, wz);

        var exists = store.TryGetChunk(expectedChunk, out var chunk);

        Assert.True(exists);
        Assert.Equal(7, chunk!.GetBlock(expectedLocal.X, expectedLocal.Y, expectedLocal.Z));
    }

    [Fact]
    public void SetBlockThenGetBlock_PersistsDataInAuthoritativeStore()
    {
        var store = new InMemoryChunkStore();
        var service = new WorldBlockService(store);

        service.SetBlock(-65, 96, 0, 42);

        var fromService = service.GetBlock(-65, 96, 0);
        var chunkCoord = CoordConverter.WorldToChunk(-65, 96, 0);
        var local = CoordConverter.WorldToLocal(-65, 96, 0);
        var fromStore = store.GetOrCreateChunk(chunkCoord).GetBlock(local.X, local.Y, local.Z);

        Assert.Equal(42, fromService);
        Assert.Equal(42, fromStore);
    }

    [Fact]
    public void SetBlock_PublishesChunkModifiedExactlyOncePerWrite()
    {
        var store = new InMemoryChunkStore();
        var events = new RecordingEventPublisher();
        var service = new WorldBlockService(store, events);

        service.SetBlock(-1, 0, 32, 1);

        var expectedChunk = CoordConverter.WorldToChunk(-1, 0, 32);
        Assert.Single(events.Events);
        Assert.Equal(new ChunkModified(expectedChunk), events.Events[0]);
    }

    private sealed class RecordingEventPublisher : IEventPublisher
    {
        public List<IDomainEvent> Events { get; } = [];

        public void Publish<TEvent>(TEvent domainEvent)
            where TEvent : IDomainEvent
        {
            Events.Add(domainEvent);
        }
    }
}
