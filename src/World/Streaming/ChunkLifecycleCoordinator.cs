using Astroblock.Core.Coords;
using Astroblock.Core.Events;
using Astroblock.Core.Interfaces;

namespace Astroblock.World.Streaming;

/// <summary>
/// Coordinates chunk lifecycle transitions by reconciling desired streaming sets
/// and emitting domain events for load/unload state changes.
/// </summary>
public sealed class ChunkLifecycleCoordinator
{
    private readonly IChunkStreamer _chunkStreamer;
    private readonly IEventPublisher _eventPublisher;
    private readonly HashSet<ChunkCoord3> _activeChunks = new();

    public ChunkLifecycleCoordinator(IChunkStreamer chunkStreamer, IEventPublisher eventPublisher)
    {
        _chunkStreamer = chunkStreamer ?? throw new ArgumentNullException(nameof(chunkStreamer));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
    }

    /// <summary>
    /// Reconciles active chunk residency against the current desired set and emits
    /// unload events before load events for deterministic lifecycle ordering.
    /// </summary>
    public void Reconcile(WorldAnchor anchor, int radius)
    {
        var desiredChunks = new HashSet<ChunkCoord3>(_chunkStreamer.ComputeDesiredSet(anchor, radius));

        foreach (var coord in _activeChunks.Except(desiredChunks).OrderBy(coord => coord.X).ThenBy(coord => coord.Y).ThenBy(coord => coord.Z))
        {
            _activeChunks.Remove(coord);
            _eventPublisher.Publish(new ChunkUnloaded(coord));
        }

        foreach (var coord in desiredChunks.Except(_activeChunks).OrderBy(coord => coord.X).ThenBy(coord => coord.Y).ThenBy(coord => coord.Z))
        {
            _activeChunks.Add(coord);
            _eventPublisher.Publish(new ChunkLoaded(coord));
        }
    }

    /// <summary>
    /// Returns a stable snapshot of the currently active chunk coordinates.
    /// </summary>
    public IReadOnlyCollection<ChunkCoord3> GetActiveChunksSnapshot() => _activeChunks.ToArray();
}
