using Astroblock.Core.Coords;

namespace Astroblock.UnitTests.Core.Coords;

public sealed class ChunkCoord3HashEqualityTests
{
    [Fact]
    public void EqualityAndHash_WhenCoordinatesMatch_AreStableForDictionaryKeys()
    {
        var keyA = ChunkCoord3.FromLongs(-1, 0, 31);
        var keyB = ChunkCoord3.FromInts(-1, 0, 31);

        Assert.True(keyA == keyB);
        Assert.True(keyA.Equals(keyB));
        Assert.Equal(keyA.GetHashCode(), keyB.GetHashCode());

        var dictionary = new Dictionary<ChunkCoord3, string>
        {
            [keyA] = "first"
        };

        dictionary[keyB] = "second";

        Assert.Single(dictionary);
        Assert.Equal("second", dictionary[keyA]);
        Assert.True(dictionary.ContainsKey(keyB));
    }

    [Fact]
    public void EqualityAndHash_WhenCoordinatesDiffer_DoNotAliasDictionaryEntries()
    {
        var a = ChunkCoord3.FromLongs(-1, -1, -1);
        var b = ChunkCoord3.FromLongs(-1, -1, 0);

        var dictionary = new Dictionary<ChunkCoord3, int>
        {
            [a] = 1,
            [b] = 2
        };

        Assert.NotEqual(a, b);
        Assert.Equal(2, dictionary.Count);
        Assert.Equal(1, dictionary[a]);
        Assert.Equal(2, dictionary[b]);
    }
}
