using Astroblock.Core.Coords;
using Astroblock.World.Generation;

namespace Astroblock.UnitTests.World.Generation;

public sealed class SimplePlanetWorldGeneratorTests
{
    [Fact]
    public void Generate_SameSeedAndCoord_ReturnsIdenticalChunkData()
    {
        var config = new SimplePlanetWorldGeneratorConfig
        {
            PlanetRadius = 96,
            SurfaceNoiseAmplitude = 1.5f,
            SurfaceNoiseFrequency = 0.05f,
            SolidBlockValue = 3,
        };

        var generator = new SimplePlanetWorldGenerator(config);
        var coord = CoordConverter.WorldToChunk(64, -32, 96);

        var first = generator.Generate(coord, seed: 2026);
        var second = generator.Generate(coord, seed: 2026);

        Assert.Equal(first.Blocks, second.Blocks);
    }

    [Fact]
    public void Generate_HasSurfaceNearIntendedSpawnAltitude()
    {
        var radius = 96;
        var config = new SimplePlanetWorldGeneratorConfig
        {
            PlanetRadius = radius,
            SurfaceNoiseAmplitude = 0f,
            SurfaceNoiseFrequency = 0.1f,
            SolidBlockValue = 9,
        };

        var generator = new SimplePlanetWorldGenerator(config);
        const int seed = 1337;

        var center = ResolvePlanetCenter(seed);
        var spawnWorldY = center.y + radius;

        var foundSurfaceTransition = false;
        for (var x = center.x - 2; x <= center.x + 2 && !foundSurfaceTransition; x++)
        {
            for (var z = center.z - 2; z <= center.z + 2 && !foundSurfaceTransition; z++)
            {
                var below = SampleWorldBlock(generator, seed, (int)x, (int)(spawnWorldY - 1), (int)z);
                var atSurface = SampleWorldBlock(generator, seed, (int)x, (int)spawnWorldY, (int)z);
                var above = SampleWorldBlock(generator, seed, (int)x, (int)(spawnWorldY + 1), (int)z);

                foundSurfaceTransition = below == config.SolidBlockValue
                    && atSurface == config.SolidBlockValue
                    && above == 0;
            }
        }

        Assert.True(foundSurfaceTransition, "Expected to find solid terrain with open space just above near spawn altitude.");
    }

    [Fact]
    public void Constructor_WithInvalidConfig_ThrowsArgumentOutOfRangeException()
    {
        var invalidConfig = new SimplePlanetWorldGeneratorConfig
        {
            PlanetRadius = 0,
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new SimplePlanetWorldGenerator(invalidConfig));
    }

    private static byte SampleWorldBlock(SimplePlanetWorldGenerator generator, int seed, int wx, int wy, int wz)
    {
        var coord = CoordConverter.WorldToChunk(wx, wy, wz);
        var local = CoordConverter.WorldToLocal(wx, wy, wz);
        var chunk = generator.Generate(coord, seed);
        return chunk.GetBlock(local.X, local.Y, local.Z);
    }

    private static (long x, long y, long z) ResolvePlanetCenter(int seed)
    {
        var x = ToRange(Hash(seed, 0, 0, 0, 101), -96, 96);
        var y = ToRange(Hash(seed, 0, 0, 0, 102), -96, 96);
        var z = ToRange(Hash(seed, 0, 0, 0, 103), -96, 96);
        return (x, y, z);
    }

    private static int ToRange(int value, int minInclusive, int maxInclusive)
    {
        if (minInclusive == maxInclusive)
        {
            return minInclusive;
        }

        var span = (maxInclusive - minInclusive) + 1;
        return minInclusive + (int)((uint)value % (uint)span);
    }

    private static int Hash(int seed, long x, long y, long z, int salt)
    {
        unchecked
        {
            ulong h = (uint)seed;
            h ^= Mix((ulong)x + (ulong)(uint)salt * 0x9E3779B97F4A7C15UL);
            h ^= Mix((ulong)y + 0xC2B2AE3D27D4EB4FUL);
            h ^= Mix((ulong)z + 0x165667B19E3779F9UL);
            h ^= Mix((ulong)(uint)salt + 0x85EBCA77C2B2AE63UL);
            h = Mix(h);
            return (int)(h & 0x7FFFFFFF);
        }
    }

    private static ulong Mix(ulong value)
    {
        value ^= value >> 33;
        value *= 0xFF51AFD7ED558CCDUL;
        value ^= value >> 33;
        value *= 0xC4CEB9FE1A85EC53UL;
        value ^= value >> 33;
        return value;
    }
}
