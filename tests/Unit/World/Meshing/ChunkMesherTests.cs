using Astroblock.Core.Coords;
using Astroblock.World.Chunks;
using Astroblock.World.Meshing;

namespace Astroblock.UnitTests.World.Meshing;

public sealed class ChunkMesherTests
{
    [Fact]
    public void Build_WhenChunkIsEmpty_EmitsNoGeometry()
    {
        var chunk = new ChunkData();
        var sut = new ChunkMesher();

        var mesh = sut.Build(chunk, ChunkCoord3.FromInts(0, 0, 0));

        Assert.Empty(mesh.Positions);
        Assert.Empty(mesh.Normals);
        Assert.Empty(mesh.Indices);
        Assert.Empty(mesh.Uvs);
    }

    [Fact]
    public void Build_WhenChunkHasSingleSolidBlock_EmitsSixFaces()
    {
        var chunk = new ChunkData();
        chunk.SetBlock(0, 0, 0, 1);
        var sut = new ChunkMesher();

        var mesh = sut.Build(chunk, ChunkCoord3.FromInts(1, 0, 0));

        Assert.Equal(24, mesh.Positions.Count);
        Assert.Equal(24, mesh.Normals.Count);
        Assert.Equal(36, mesh.Indices.Count);
        Assert.Equal(24, mesh.Uvs.Count);

        Assert.Equal(new MeshVector3(1f, 0f, 0f), mesh.Normals[0]);
        Assert.Equal(32f + 1f, mesh.Positions[0].X);
        Assert.Equal(0f, mesh.Positions[0].Y);
        Assert.Equal(0f, mesh.Positions[0].Z);
        Assert.Equal(new[] { 0, 1, 2, 0, 2, 3 }, mesh.Indices.Take(6));
    }

    [Fact]
    public void Build_WhenTwoAdjacentBlocksShareFace_CullsInteriorFaces()
    {
        var chunk = new ChunkData();
        chunk.SetBlock(0, 0, 0, 1);
        chunk.SetBlock(1, 0, 0, 1);
        var sut = new ChunkMesher();

        var mesh = sut.Build(chunk, ChunkCoord3.FromInts(0, 0, 0));

        Assert.Equal(40, mesh.Positions.Count);
        Assert.Equal(40, mesh.Normals.Count);
        Assert.Equal(60, mesh.Indices.Count);
        Assert.Equal(40, mesh.Uvs.Count);
    }
}
