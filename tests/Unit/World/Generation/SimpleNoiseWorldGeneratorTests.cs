using Astroblock.Core.Coords;
using Astroblock.World.Generation;

namespace Astroblock.UnitTests.World.Generation;

public sealed class SimpleNoiseWorldGeneratorTests
{
    [Fact]
    public void Generate_WithSameSeedAndCoord_IsDeterministic()
    {
        var generator = new SimpleNoiseWorldGenerator();
        var coord = new ChunkCoord3(4, -3, 9);

        var first = generator.Generate(coord, seed: 1337);
        var second = generator.Generate(coord, seed: 1337);

        Assert.Equal(first.Blocks, second.Blocks);
    }

    [Fact]
    public void Generate_WithDifferentSeeds_CanProduceDifferentResults()
    {
        var config = new SimpleNoiseWorldGeneratorConfig
        {
            ChunkSpawnThreshold = 255,
            MinAsteroidRadius = 6,
            MaxAsteroidRadius = 12,
        };

        var generator = new SimpleNoiseWorldGenerator(config);
        var coord = new ChunkCoord3(0, 0, 0);

        var first = generator.Generate(coord, seed: 1);
        var second = generator.Generate(coord, seed: 2);

        Assert.NotEqual(first.Blocks, second.Blocks);
    }

    [Fact]
    public void Generate_DefaultConfig_ProducesMostlyEmptySpace()
    {
        var generator = new SimpleNoiseWorldGenerator();
        var totalChunks = 256;
        var chunksWithSolids = 0;

        for (var i = 0; i < totalChunks; i++)
        {
            var coord = new ChunkCoord3(i, 0, 0);
            var generated = generator.Generate(coord, seed: 99);
            var hasSolid = Array.Exists(generated.Blocks, block => block != 0);
            if (hasSolid)
            {
                chunksWithSolids++;
            }
        }

        Assert.InRange(chunksWithSolids, 1, 32);
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
}
