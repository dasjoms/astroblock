using Astroblock.Core.Coords;
using Astroblock.Core.Events;
using Astroblock.Core.Interfaces;
using Astroblock.World;
using Astroblock.World.Chunks;
using Astroblock.World.Generation;
using Astroblock.World.Storage;
using Astroblock.World.Streaming;

namespace Astroblock.UnitTests.World;

public sealed class WorldCoordinatorTests
{
    [Fact]
    public void Update_WhenDesiredChunkMissing_GeneratesAndStoresChunkAndPublishesLoad()
    {
        var desired = ChunkCoord3.FromLongs(2, 1, -3);
        var streamer = new StubChunkStreamer([desired]);
        var store = new InMemoryChunkStore();
        var generator = new RecordingWorldGenerator();
        var events = new RecordingEventPublisher();
        var coordinator = new WorldCoordinator(streamer, store, generator, streamingRadius: 1, worldSeed: 12345, events);

        coordinator.Update(new WorldAnchor(0, 0, 0));

        Assert.Collection(
            generator.Calls,
            call =>
            {
                Assert.Equal(desired, call.coord);
                Assert.Equal(12345, call.seed);
            });

        Assert.True(store.TryGetChunk(desired, out _));
        Assert.Collection(
            events.Events,
            evt => Assert.Equal(new ChunkLoaded(desired), evt));
    }

    [Fact]
    public void Update_WhenStoredChunkNoLongerDesired_RemovesAndPublishesUnload()
    {
        var keep = ChunkCoord3.FromLongs(0, 0, 0);
        var remove = ChunkCoord3.FromLongs(1, 0, 0);
        var streamer = new StubChunkStreamer([keep]);
        var store = new InMemoryChunkStore();
        store.SetChunk(keep, new ChunkData());
        store.SetChunk(remove, new ChunkData());

        var events = new RecordingEventPublisher();
        var coordinator = new WorldCoordinator(streamer, store, new RecordingWorldGenerator(), 1, 77, events);

        coordinator.Update(new WorldAnchor(0, 0, 0));

        Assert.True(store.TryGetChunk(keep, out _));
        Assert.False(store.TryGetChunk(remove, out _));
        Assert.Collection(events.Events, evt => Assert.Equal(new ChunkUnloaded(remove), evt));
    }

    [Fact]
    public void Update_SameInputsProduceDeterministicGenerationOrder()
    {
        var coords = new[]
        {
            ChunkCoord3.FromLongs(5, 0, 0),
            ChunkCoord3.FromLongs(-1, 2, 3),
            ChunkCoord3.FromLongs(0, 0, 0)
        };

        var run1 = RunGenerationAndCaptureOrder(coords);
        var run2 = RunGenerationAndCaptureOrder(coords);

        Assert.Equal(run1, run2);
    }

    private static IReadOnlyList<ChunkCoord3> RunGenerationAndCaptureOrder(IReadOnlyCollection<ChunkCoord3> desired)
    {
        var streamer = new StubChunkStreamer(desired);
        var store = new InMemoryChunkStore();
        var generator = new RecordingWorldGenerator();
        var coordinator = new WorldCoordinator(streamer, store, generator, 2, 999);

        coordinator.Update(new WorldAnchor(128, 128, 128));

        return generator.Calls.Select(call => call.coord).ToArray();
    }

    private sealed class StubChunkStreamer : IChunkStreamer
    {
        private readonly IReadOnlyCollection<ChunkCoord3> _desired;

        public StubChunkStreamer(IReadOnlyCollection<ChunkCoord3> desired)
        {
            _desired = desired;
        }

        public IReadOnlyCollection<ChunkCoord3> ComputeDesiredSet(WorldAnchor anchor, int radius) => _desired;
    }

    private sealed class RecordingWorldGenerator : IWorldGenerator
    {
        public List<(ChunkCoord3 coord, int seed)> Calls { get; } = [];

        public ChunkData Generate(ChunkCoord3 coord, int seed)
        {
            Calls.Add((coord, seed));
            return new ChunkData();
        }
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
