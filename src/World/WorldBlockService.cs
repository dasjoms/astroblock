using Astroblock.Core.Coords;
using Astroblock.Core.Events;
using Astroblock.Core.Interfaces;

namespace Astroblock.World;

/// <summary>
/// World-space voxel mutation/query service backed by authoritative chunk storage.
/// </summary>
public sealed class WorldBlockService
{
    private readonly IChunkStore _chunkStore;
    private readonly IEventPublisher? _eventPublisher;

    public WorldBlockService(IChunkStore chunkStore, IEventPublisher? eventPublisher = null)
    {
        _chunkStore = chunkStore ?? throw new ArgumentNullException(nameof(chunkStore));
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Reads a block value at world-space coordinates.
    /// Missing chunks resolve to air (0).
    /// </summary>
    public byte GetBlock(int wx, int wy, int wz)
    {
        var chunkCoord = CoordConverter.WorldToChunk(wx, wy, wz);
        if (!_chunkStore.TryGetChunk(chunkCoord, out var chunk))
        {
            return 0;
        }

        var local = CoordConverter.WorldToLocal(wx, wy, wz);
        return chunk.GetBlock(local.X, local.Y, local.Z);
    }

    /// <summary>
    /// Writes a block value at world-space coordinates and publishes a chunk-modified event.
    /// </summary>
    public void SetBlock(int wx, int wy, int wz, byte value)
    {
        var chunkCoord = CoordConverter.WorldToChunk(wx, wy, wz);
        var local = CoordConverter.WorldToLocal(wx, wy, wz);

        var chunk = _chunkStore.GetOrCreateChunk(chunkCoord);
        chunk.SetBlock(local.X, local.Y, local.Z, value);

        _eventPublisher?.Publish(new ChunkModified(chunkCoord));
    }
}
