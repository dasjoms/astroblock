using Astroblock.Core.Coords;
using Astroblock.Core.Events;
using Astroblock.Core.Interfaces;
using Astroblock.World.Streaming;

namespace Astroblock.UnitTests.World.Streaming;

public sealed class ChunkLifecycleCoordinatorTests
{
    [Fact]
    public void Reconcile_FirstPass_EmitsChunkLoadedForAllDesiredChunks()
    {
        var eventPublisher = new RecordingEventPublisher();
        var streamer = new StubChunkStreamer(
        [
            ChunkCoord3.FromLongs(0, 0, 0),
            ChunkCoord3.FromLongs(1, 0, 0)
        ]);

        var coordinator = new ChunkLifecycleCoordinator(streamer, eventPublisher);

        coordinator.Reconcile(new WorldAnchor(0, 0, 0), 0);

        Assert.Collection(
            eventPublisher.Events,
            evt => Assert.Equal(new ChunkLoaded(ChunkCoord3.FromLongs(0, 0, 0)), evt),
            evt => Assert.Equal(new ChunkLoaded(ChunkCoord3.FromLongs(1, 0, 0)), evt));
    }

    [Fact]
    public void Reconcile_WhenDesiredSetChanges_EmitsUnloadsBeforeLoads()
    {
        var eventPublisher = new RecordingEventPublisher();
        var streamer = new SequencedChunkStreamer(
        [ChunkCoord3.FromLongs(0, 0, 0), ChunkCoord3.FromLongs(1, 0, 0)],
        [ChunkCoord3.FromLongs(1, 0, 0), ChunkCoord3.FromLongs(2, 0, 0)]);

        var coordinator = new ChunkLifecycleCoordinator(streamer, eventPublisher);

        coordinator.Reconcile(new WorldAnchor(0, 0, 0), 0);
        eventPublisher.Clear();

        coordinator.Reconcile(new WorldAnchor(0, 0, 0), 0);

        Assert.Collection(
            eventPublisher.Events,
            evt => Assert.Equal(new ChunkUnloaded(ChunkCoord3.FromLongs(0, 0, 0)), evt),
            evt => Assert.Equal(new ChunkLoaded(ChunkCoord3.FromLongs(2, 0, 0)), evt));
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

    private sealed class StubChunkStreamer : IChunkStreamer
    {
        private readonly IReadOnlyCollection<ChunkCoord3> _desired;

        public StubChunkStreamer(IReadOnlyCollection<ChunkCoord3> desired)
        {
            _desired = desired;
        }

        public IReadOnlyCollection<ChunkCoord3> ComputeDesiredSet(WorldAnchor anchor, int radius) => _desired;
    }

    private sealed class SequencedChunkStreamer : IChunkStreamer
    {
        private readonly Queue<IReadOnlyCollection<ChunkCoord3>> _sequence;

        public SequencedChunkStreamer(params IReadOnlyCollection<ChunkCoord3>[] sequence)
        {
            _sequence = new Queue<IReadOnlyCollection<ChunkCoord3>>(sequence);
        }

        public IReadOnlyCollection<ChunkCoord3> ComputeDesiredSet(WorldAnchor anchor, int radius)
        {
            if (_sequence.Count > 1)
            {
                return _sequence.Dequeue();
            }

            return _sequence.Peek();
        }
    }
}
