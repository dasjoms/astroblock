using Astroblock.Core.Coords;
using Astroblock.Core.Events;
using Astroblock.Core.Interfaces;
using Astroblock.World.Generation;
using Astroblock.World.Streaming;

namespace Astroblock.World;

/// <summary>
/// First-pass world orchestration loop that reconciles streamed chunk residency
/// with authoritative storage and deterministic generation.
/// </summary>
public sealed class WorldCoordinator
{
    private readonly IChunkStreamer _chunkStreamer;
    private readonly IChunkStore _chunkStore;
    private readonly IWorldGenerator _worldGenerator;
    private readonly IEventPublisher? _eventPublisher;
    private readonly int _streamingRadius;
    private readonly int _worldSeed;

    public WorldCoordinator(
        IChunkStreamer chunkStreamer,
        IChunkStore chunkStore,
        IWorldGenerator worldGenerator,
        int streamingRadius,
        int worldSeed,
        IEventPublisher? eventPublisher = null)
    {
        _chunkStreamer = chunkStreamer ?? throw new ArgumentNullException(nameof(chunkStreamer));
        _chunkStore = chunkStore ?? throw new ArgumentNullException(nameof(chunkStore));
        _worldGenerator = worldGenerator ?? throw new ArgumentNullException(nameof(worldGenerator));

        if (streamingRadius < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(streamingRadius), streamingRadius, "Streaming radius must be non-negative.");
        }

        _streamingRadius = streamingRadius;
        _worldSeed = worldSeed;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Reconciles world storage against the streamer's desired chunk set.
    /// For each missing desired chunk, generates deterministic payload and inserts into store.
    /// For each non-desired resident chunk, removes it from store.
    ///
    /// Iteration order is stable to preserve deterministic behavior for identical
    /// input anchor sequences and world seed.
    /// </summary>
    public void Update(WorldAnchor anchor)
    {
        var desiredChunks = new HashSet<ChunkCoord3>(_chunkStreamer.ComputeDesiredSet(anchor, _streamingRadius));

        foreach (var coord in SortByCoord(desiredChunks))
        {
            if (_chunkStore.TryGetChunk(coord, out _))
            {
                continue;
            }

            var generatedChunk = _worldGenerator.Generate(coord, _worldSeed);
            _chunkStore.SetChunk(coord, generatedChunk);
            _eventPublisher?.Publish(new ChunkLoaded(coord));
        }

        var existingChunks = _chunkStore.GetStoredChunkCoordsSnapshot();
        foreach (var coord in SortByCoord(existingChunks.Where(coord => !desiredChunks.Contains(coord))))
        {
            if (_chunkStore.RemoveChunk(coord))
            {
                _eventPublisher?.Publish(new ChunkUnloaded(coord));
            }
        }

        // Extension point (future): move generation and eviction into async job queues,
        // then commit queue results in deterministic coordinate order.
    }

    private static IReadOnlyList<ChunkCoord3> SortByCoord(IEnumerable<ChunkCoord3> coords)
        => coords.OrderBy(static coord => coord.X).ThenBy(static coord => coord.Y).ThenBy(static coord => coord.Z).ToArray();
}
