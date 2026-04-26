using Astroblock.Core.Coords;
using Astroblock.World.Chunks;
using Astroblock.World.Storage;

namespace Astroblock.UnitTests.World.Storage;

public sealed class InMemoryChunkStoreTests
{
    [Fact]
    public void GetOrCreateChunk_WhenChunkMissing_CreatesAndThenReturnsSameInstance()
    {
        var store = new InMemoryChunkStore();
        var coord = CoordConverter.WorldToChunk(0, 0, 0);

        var first = store.GetOrCreateChunk(coord);
        var second = store.GetOrCreateChunk(coord);

        Assert.Same(first, second);
    }

    [Fact]
    public void SetChunkThenTryGetChunk_WhenCoordinateExists_ReturnsStoredChunk()
    {
        var store = new InMemoryChunkStore();
        var coord = CoordConverter.WorldToChunk(-1, 32, -33);
        var chunk = new ChunkData();

        store.SetChunk(coord, chunk);

        var found = store.TryGetChunk(coord, out var resolved);
        Assert.True(found);
        Assert.Same(chunk, resolved);
    }

    [Fact]
    public void RemoveChunk_WhenCoordinateExists_RemovesAndReturnsTrue()
    {
        var store = new InMemoryChunkStore();
        var coord = CoordConverter.WorldToChunk(31, 31, 31);
        store.SetChunk(coord, new ChunkData());

        var removed = store.RemoveChunk(coord);
        var existsAfter = store.TryGetChunk(coord, out _);

        Assert.True(removed);
        Assert.False(existsAfter);
    }

    [Fact]
    public void RemoveChunk_WhenCoordinateMissing_ReturnsFalse()
    {
        var store = new InMemoryChunkStore();
        var coord = CoordConverter.WorldToChunk(64, 64, 64);

        var removed = store.RemoveChunk(coord);

        Assert.False(removed);
    }
}
