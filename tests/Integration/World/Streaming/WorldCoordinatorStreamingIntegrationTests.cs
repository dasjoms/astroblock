using Astroblock.Core.Coords;
using Astroblock.Core.Events;
using Astroblock.Core.Interfaces;
using Astroblock.World;
using Astroblock.World.Generation;
using Astroblock.World.Storage;
using Astroblock.World.Streaming;

namespace Astroblock.IntegrationTests.World.Streaming;

public sealed class WorldCoordinatorStreamingIntegrationTests
{
    [Fact]
    public void Update_AcrossChunkBoundary_ReconcilesLoadUnloadAndPreservesDeterministicGeneratedChunkData()
    {
        const int seed = 424242;
        const int radius = 1;
        var anchorA = new WorldAnchor(0, 0, 0);
        var anchorB = new WorldAnchor(ChunkConstants.ChunkSize, 0, 0);

        var streamer = new CubeChunkStreamer();
        var generator = new SimpleNoiseWorldGenerator();
        var store = new InMemoryChunkStore();
        var events = new RecordingEventPublisher();
        var coordinator = new WorldCoordinator(streamer, store, generator, radius, seed, events);

        coordinator.Update(anchorA);
        var loadedAtA = store.GetStoredChunkCoordsSnapshot().ToHashSet();

        events.Clear();
        coordinator.Update(anchorB);

        var loadedAtB = store.GetStoredChunkCoordsSnapshot().ToHashSet();
        var expectedOverlap = loadedAtA.Intersect(loadedAtB).ToHashSet();
        var expectedUnloads = loadedAtA.Except(loadedAtB).ToHashSet();
        var expectedLoads = loadedAtB.Except(loadedAtA).ToHashSet();

        var actualUnloads = events.Events
            .OfType<ChunkUnloaded>()
            .Select(evt => evt.Coord)
            .ToHashSet();
        var actualLoads = events.Events
            .OfType<ChunkLoaded>()
            .Select(evt => evt.Coord)
            .ToHashSet();

        Assert.Equal(18, expectedOverlap.Count);
        Assert.Equal(9, expectedUnloads.Count);
        Assert.Equal(9, expectedLoads.Count);

        Assert.Equal(expectedUnloads, actualUnloads);
        Assert.Equal(expectedLoads, actualLoads);

        var sampledChunkCoord = ChunkCoord3.FromLongs(2, 0, 0);
        Assert.Contains(sampledChunkCoord, expectedLoads);
        var sampledChunk = AssertChunkExists(store, sampledChunkCoord);

        var freshStore = new InMemoryChunkStore();
        var freshCoordinator = new WorldCoordinator(
            new CubeChunkStreamer(),
            freshStore,
            new SimpleNoiseWorldGenerator(),
            radius,
            seed);

        freshCoordinator.Update(anchorB);
        var sampledChunkFromFreshCoordinator = AssertChunkExists(freshStore, sampledChunkCoord);

        Assert.Equal(sampledChunk.Blocks, sampledChunkFromFreshCoordinator.Blocks);
    }

    private static Astroblock.World.Chunks.ChunkData AssertChunkExists(InMemoryChunkStore store, ChunkCoord3 coord)
    {
        return store.TryGetChunk(coord, out var chunk)
            ? chunk
            : throw new Xunit.Sdk.XunitException($"Expected chunk {coord} to exist in store.");
    }

    private sealed class RecordingEventPublisher : IEventPublisher
    {
        public List<IDomainEvent> Events { get; } = [];

        public void Publish<TEvent>(TEvent domainEvent)
            where TEvent : IDomainEvent
        {
            Events.Add(domainEvent);
        }

        public void Clear() => Events.Clear();
    }
}
