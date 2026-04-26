using Astroblock.Core.Coords;
using Astroblock.World.Chunks;

namespace Astroblock.UnitTests.World.Chunks;

public sealed class ChunkDataTests
{
    [Fact]
    public void Constructor_WhenBlockArrayLengthIsInvalid_ThrowsArgumentException()
    {
        var invalid = new byte[ChunkData.BlockCount - 1];

        Assert.Throws<ArgumentException>(() => _ = new ChunkData(invalid));
    }

    [Fact]
    public void SetBlockAndGetBlock_WhenUsingValidLocalCoordinates_RoundTripVoxelValue()
    {
        var chunk = new ChunkData();
        var value = (byte)7;

        chunk.SetBlock(31, 0, 0, value);

        Assert.Equal(value, chunk.GetBlock(31, 0, 0));
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(0, 32, 0)]
    [InlineData(0, 0, 99)]
    public void SetBlock_WhenLocalCoordinateIsOutOfRange_ThrowsArgumentOutOfRangeException(int lx, int ly, int lz)
    {
        var chunk = new ChunkData();

        Assert.Throws<ArgumentOutOfRangeException>(() => chunk.SetBlock(lx, ly, lz, 1));
    }

    [Fact]
    public void BlockCount_MatchesChunkConstantsVolume()
    {
        var expected = ChunkConstants.ChunkSize * ChunkConstants.ChunkSize * ChunkConstants.ChunkSize;
        Assert.Equal(ChunkData.BlockCount, expected);
    }
}
