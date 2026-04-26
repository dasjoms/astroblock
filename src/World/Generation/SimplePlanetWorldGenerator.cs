using Astroblock.Core.Coords;
using Astroblock.World.Chunks;

namespace Astroblock.World.Generation;

/// <summary>
/// Deterministic generator that creates one large spherical planet and empty space elsewhere.
/// </summary>
public sealed class SimplePlanetWorldGenerator : IWorldGenerator
{
    private readonly SimplePlanetWorldGeneratorConfig _config;

    public SimplePlanetWorldGenerator(SimplePlanetWorldGeneratorConfig? config = null)
    {
        _config = config ?? new SimplePlanetWorldGeneratorConfig();
        _config.Validate();
    }

    public ChunkData Generate(ChunkCoord3 coord, int seed)
    {
        var chunk = new ChunkData();
        var chunkSize = ChunkConstants.ChunkSize;

        var worldOriginX = coord.X * chunkSize;
        var worldOriginY = coord.Y * chunkSize;
        var worldOriginZ = coord.Z * chunkSize;

        var (centerX, centerY, centerZ) = ResolvePlanetCenter(seed);

        for (var lz = 0; lz < chunkSize; lz++)
        {
            for (var ly = 0; ly < chunkSize; ly++)
            {
                for (var lx = 0; lx < chunkSize; lx++)
                {
                    var wx = worldOriginX + lx;
                    var wy = worldOriginY + ly;
                    var wz = worldOriginZ + lz;

                    var dx = wx - centerX;
                    var dy = wy - centerY;
                    var dz = wz - centerZ;

                    var distance = Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
                    var surfaceNoise = SampleSignedNoise(seed, wx, wy, wz) * _config.SurfaceNoiseAmplitude;
                    var threshold = _config.PlanetRadius + surfaceNoise;

                    if (distance <= threshold)
                    {
                        chunk.SetBlock(lx, ly, lz, _config.SolidBlockValue);
                    }
                }
            }
        }

        return chunk;
    }

    private static (long x, long y, long z) ResolvePlanetCenter(int seed)
    {
        // v0 baseline: deterministic seed-derived center near origin.
        var x = ToRange(Hash(seed, 0, 0, 0, 101), -96, 96);
        var y = ToRange(Hash(seed, 0, 0, 0, 102), -96, 96);
        var z = ToRange(Hash(seed, 0, 0, 0, 103), -96, 96);
        return (x, y, z);
    }

    private float SampleSignedNoise(int seed, long x, long y, long z)
    {
        var fx = (long)Math.Floor(x * _config.SurfaceNoiseFrequency);
        var fy = (long)Math.Floor(y * _config.SurfaceNoiseFrequency);
        var fz = (long)Math.Floor(z * _config.SurfaceNoiseFrequency);
        var hash = Hash(seed, fx, fy, fz, 17);
        var unit = (hash & 0xFFFF) / 65535f;
        return (unit * 2f) - 1f;
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
