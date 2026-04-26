using Astroblock.Core.Coords;
using Astroblock.Core.Events;
using Astroblock.Core.Interfaces;
using Astroblock.World;
using Astroblock.World.Chunks;
using Astroblock.World.Generation;
using Astroblock.World.Storage;
using Astroblock.World.Streaming;

namespace Astroblock.UnitTests.World.Streaming;

public sealed class CubeChunkStreamerTests
{
    [Fact]
    public void ComputeDesiredSet_RadiusZero_ReturnsExactlyAnchorChunk()
    {
        var streamer = new CubeChunkStreamer();
        var anchor = new WorldAnchor(-1, -32, -33);
        var expectedAnchorChunk = CoordConverter.WorldToChunk(anchor.WorldX, anchor.WorldY, anchor.WorldZ);

        var desired = streamer.ComputeDesiredSet(anchor, 0);

        Assert.Single(desired);
        Assert.Contains(expectedAnchorChunk, desired);
    }

    [Fact]
    public void ComputeDesiredSet_RadiusOne_ReturnsExactlyTwentySevenChunkCoords()
    {
        var streamer = new CubeChunkStreamer();
        var anchor = new WorldAnchor(0, 0, 0);
        var center = CoordConverter.WorldToChunk(anchor.WorldX, anchor.WorldY, anchor.WorldZ);

        var desired = streamer.ComputeDesiredSet(anchor, 1);

        Assert.Equal(27, desired.Count);
        Assert.Contains(center, desired);
        Assert.Contains(ChunkCoord3.FromLongs(center.X - 1, center.Y - 1, center.Z - 1), desired);
        Assert.Contains(ChunkCoord3.FromLongs(center.X + 1, center.Y + 1, center.Z + 1), desired);
    }

    [Fact]
    public void CoordinatorUpdate_LoadsAndUnloadsExpectedCoords()
    {
        var streamer = new SequenceChunkStreamer(
            [CoordConverter.WorldToChunk(0, 0, 0)],
            [CoordConverter.WorldToChunk(ChunkConstants.ChunkSize, 0, 0)]);

        var store = new InMemoryChunkStore();
        var events = new RecordingEventPublisher();
        var coordinator = new WorldCoordinator(streamer, store, new StubWorldGenerator(), streamingRadius: 0, worldSeed: 9, events);

        coordinator.Update(new WorldAnchor(0, 0, 0));
        events.Clear();

        coordinator.Update(new WorldAnchor(ChunkConstants.ChunkSize, 0, 0));

        var firstChunk = CoordConverter.WorldToChunk(0, 0, 0);
        var secondChunk = CoordConverter.WorldToChunk(ChunkConstants.ChunkSize, 0, 0);

        Assert.False(store.TryGetChunk(firstChunk, out _));
        Assert.True(store.TryGetChunk(secondChunk, out _));
        Assert.Collection(
            events.Events,
            evt => Assert.Equal(new ChunkUnloaded(firstChunk), evt),
            evt => Assert.Equal(new ChunkLoaded(secondChunk), evt));
    }

    [Fact]
    public void ComputeDesiredSet_WhenRadiusIsNegative_Throws()
    {
        var streamer = new CubeChunkStreamer();

        Assert.Throws<ArgumentOutOfRangeException>(() => streamer.ComputeDesiredSet(new WorldAnchor(0, 0, 0), -1));
    }

    private sealed class SequenceChunkStreamer : IChunkStreamer
    {
        private readonly Queue<IReadOnlyCollection<ChunkCoord3>> _desiredByUpdate;

        public SequenceChunkStreamer(params IReadOnlyCollection<ChunkCoord3>[] desiredByUpdate)
        {
            _desiredByUpdate = new Queue<IReadOnlyCollection<ChunkCoord3>>(desiredByUpdate);
        }

        public IReadOnlyCollection<ChunkCoord3> ComputeDesiredSet(WorldAnchor anchor, int radius)
            => _desiredByUpdate.Count > 1 ? _desiredByUpdate.Dequeue() : _desiredByUpdate.Peek();
    }

    private sealed class StubWorldGenerator : IWorldGenerator
    {
        public ChunkData Generate(ChunkCoord3 coord, int seed) => new();
    }

    private sealed class RecordingEventPublisher : IEventPublisher
    {
        public List<IDomainEvent> Events { get; } = [];

        public void Publish<TEvent>(TEvent domainEvent)
            where TEvent : IDomainEvent
            => Events.Add(domainEvent);

        public void Clear() => Events.Clear();
    }
}
