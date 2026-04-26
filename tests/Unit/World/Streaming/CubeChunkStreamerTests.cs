using Astroblock.Core.Coords;
using Astroblock.World.Streaming;

namespace Astroblock.UnitTests.World.Streaming;

public sealed class CubeChunkStreamerTests
{
    [Fact]
    public void ComputeDesiredSet_WhenRadiusIsOne_ReturnsCentered3x3x3ChunkCube()
    {
        var streamer = new CubeChunkStreamer();
        var anchor = new WorldAnchor(0, 0, 0);

        var desired = streamer.ComputeDesiredSet(anchor, 1);

        Assert.Equal(27, desired.Count);
        Assert.Contains(ChunkCoord3.FromLongs(0, 0, 0), desired);
        Assert.Contains(ChunkCoord3.FromLongs(-1, -1, -1), desired);
        Assert.Contains(ChunkCoord3.FromLongs(1, 1, 1), desired);
    }

    [Fact]
    public void ComputeDesiredSet_WhenAnchorIsNegativeBoundary_UsesCoordConverterChunkAnchor()
    {
        var streamer = new CubeChunkStreamer();
        var anchor = new WorldAnchor(-1, -32, -33);

        var desired = streamer.ComputeDesiredSet(anchor, 0);

        Assert.Single(desired);
        Assert.Contains(ChunkCoord3.FromLongs(-1, -1, -2), desired);
    }

    [Fact]
    public void ComputeDesiredSet_WhenRadiusIsNegative_Throws()
    {
        var streamer = new CubeChunkStreamer();

        Assert.Throws<ArgumentOutOfRangeException>(() => streamer.ComputeDesiredSet(new WorldAnchor(0, 0, 0), -1));
    }
}
