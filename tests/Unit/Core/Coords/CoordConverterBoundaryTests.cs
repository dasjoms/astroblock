using Astroblock.Core.Coords;

namespace Astroblock.UnitTests.Core.Coords;

public sealed class CoordConverterBoundaryTests
{
    private static readonly int[] BoundaryAxes =
    [
        -65, -33, -32, -31, -1, 0, 1, 31, 32, 33, 64
    ];

    public static IEnumerable<object[]> AxisBoundaryCases()
        => BoundaryAxes.Select(axis => new object[] { axis, ExpectedChunk(axis), ExpectedLocal(axis) });

    public static IEnumerable<object[]> RoundTrip3DBoundaryCases()
    {
        foreach (var x in BoundaryAxes)
        {
            foreach (var y in BoundaryAxes)
            {
                foreach (var z in BoundaryAxes)
                {
                    yield return new object[] { x, y, z };
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(AxisBoundaryCases))]
    public void WorldToChunkAndLocal_WhenAxisIsOnBoundaryRule_FollowsFloorDivisionAndZeroTo31LocalRange(
        int worldAxis,
        int expectedChunk,
        int expectedLocal)
    {
        var actualChunk = CoordConverter.WorldToChunk(worldAxis);
        var actualLocal = CoordConverter.WorldToLocal(worldAxis);

        Assert.Equal(expectedChunk, actualChunk);
        Assert.Equal(expectedLocal, actualLocal);
        Assert.InRange(actualLocal, 0, ChunkConstants.ChunkSize - 1);
    }

    [Theory]
    [MemberData(nameof(RoundTrip3DBoundaryCases))]
    public void WorldToChunkAndLocal_When3DCoordinatesHitBoundaryRule_AlwaysRoundTripAndKeepLocalAxesInZeroTo31(
        int wx,
        int wy,
        int wz)
    {
        var chunk = CoordConverter.WorldToChunk(wx, wy, wz);
        var local = CoordConverter.WorldToLocal(wx, wy, wz);

        Assert.InRange(local.X, 0, ChunkConstants.ChunkSize - 1);
        Assert.InRange(local.Y, 0, ChunkConstants.ChunkSize - 1);
        Assert.InRange(local.Z, 0, ChunkConstants.ChunkSize - 1);

        Assert.Equal(ExpectedChunk(wx), chunk.X);
        Assert.Equal(ExpectedChunk(wy), chunk.Y);
        Assert.Equal(ExpectedChunk(wz), chunk.Z);

        Assert.Equal(ExpectedLocal(wx), local.X);
        Assert.Equal(ExpectedLocal(wy), local.Y);
        Assert.Equal(ExpectedLocal(wz), local.Z);

        var worldFromChunkLocal = CoordConverter.ChunkLocalToWorld(chunk, local.X, local.Y, local.Z);
        Assert.Equal((wx, wy, wz), worldFromChunkLocal);

        var worldRoundTrip = CoordConverter.RoundTripWorldToChunkLocalToWorld(wx, wy, wz);
        Assert.Equal((wx, wy, wz), worldRoundTrip);

        var chunkLocalRoundTrip = CoordConverter.RoundTripChunkLocalToWorldToChunkLocal(
            chunk.X,
            chunk.Y,
            chunk.Z,
            local.X,
            local.Y,
            local.Z);

        Assert.Equal(chunk, chunkLocalRoundTrip.Chunk);
        Assert.Equal(local, chunkLocalRoundTrip.Local);
    }

    [Fact]
    public void PublicCoordApi_WhenBoundaryRuleApplies_IsSufficientWithoutDuplicatingMathOutsideCoreCoords()
    {
        const int wx = -33;
        const int wy = 32;
        const int wz = -1;

        var chunk = CoordConverter.WorldToChunk(wx, wy, wz);
        var local = CoordConverter.WorldToLocal(wx, wy, wz);

        // This reconstruction uses only public API and ChunkConstants.
        // If caller code can do this, no external duplicate conversion implementation is needed.
        var rebuilt = CoordConverter.ChunkLocalToWorld(chunk, local.X, local.Y, local.Z);

        Assert.Equal((-2, 1, -1), (chunk.X, chunk.Y, chunk.Z));
        Assert.Equal((31, 0, 31), local);
        Assert.Equal((wx, wy, wz), rebuilt);
    }

    private static int ExpectedChunk(int worldAxis)
        => (int)Math.Floor(worldAxis / (double)ChunkConstants.ChunkSize);

    private static int ExpectedLocal(int worldAxis)
    {
        var local = worldAxis % ChunkConstants.ChunkSize;
        return local < 0 ? local + ChunkConstants.ChunkSize : local;
    }
}
