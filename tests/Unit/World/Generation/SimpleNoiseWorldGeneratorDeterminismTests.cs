using Astroblock.Core.Coords;
using Astroblock.World.Generation;

namespace Astroblock.UnitTests.World.Generation;

public sealed class SimpleNoiseWorldGeneratorDeterminismTests
{
    [Fact]
    public void Generate_SameSeedAndCoord_ReturnsIdenticalChunkData()
    {
        var generator = new SimpleNoiseWorldGenerator();
        var coord = CoordConverter.WorldToChunk(128, -65, 999);

        var first = generator.Generate(coord, seed: 1337);
        var second = generator.Generate(coord, seed: 1337);

        Assert.Equal(first.Blocks, second.Blocks);
    }

    [Fact]
    public void Generate_DifferentSeedOrCoord_HasAtLeastOneDifferentVoxel()
    {
        var config = new SimpleNoiseWorldGeneratorConfig
        {
            ChunkSpawnThreshold = 255,
            MinAsteroidRadius = 6,
            MaxAsteroidRadius = 12,
        };

        var generator = new SimpleNoiseWorldGenerator(config);
        var coord = CoordConverter.WorldToChunk(0, 0, 0);
        var shiftedCoord = CoordConverter.WorldToChunk(ChunkConstants.ChunkSize, 0, 0);

        var seedA = generator.Generate(coord, seed: 1);
        var seedB = generator.Generate(coord, seed: 2);
        var coordB = generator.Generate(shiftedCoord, seed: 1);

        Assert.True(HasAnyDifferentVoxel(seedA.Blocks, seedB.Blocks));
        Assert.True(HasAnyDifferentVoxel(seedA.Blocks, coordB.Blocks));
    }

    [Fact]
    public void Constructor_WithInvalidConfig_ThrowsArgumentOutOfRangeException()
    {
        var invalidConfig = new SimpleNoiseWorldGeneratorConfig
        {
            MinAsteroidRadius = 8,
            MaxAsteroidRadius = 4,
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new SimpleNoiseWorldGenerator(invalidConfig));
    }

    private static bool HasAnyDifferentVoxel(byte[] left, byte[] right)
    {
        Assert.Equal(left.Length, right.Length);

        for (var i = 0; i < left.Length; i++)
        {
            if (left[i] != right[i])
            {
                return true;
            }
        }

        return false;
    }
}
