using Astroblock.Core.Coords;
using Astroblock.World.Chunks;

namespace Astroblock.World.Meshing;

/// <summary>
/// Naive exposed-face mesher for chunk voxel data.
/// Emits one quad for every solid face adjacent to air/out-of-bounds space.
/// </summary>
public sealed class ChunkMesher
{
    private static readonly FaceDescriptor[] FaceOrder =
    [
        new((1, 0, 0), new MeshVector3(1f, 0f, 0f),
            [new(1f, 0f, 0f), new(1f, 1f, 0f), new(1f, 1f, 1f), new(1f, 0f, 1f)]),
        new((-1, 0, 0), new MeshVector3(-1f, 0f, 0f),
            [new(0f, 0f, 1f), new(0f, 1f, 1f), new(0f, 1f, 0f), new(0f, 0f, 0f)]),
        new((0, 1, 0), new MeshVector3(0f, 1f, 0f),
            [new(0f, 1f, 1f), new(1f, 1f, 1f), new(1f, 1f, 0f), new(0f, 1f, 0f)]),
        new((0, -1, 0), new MeshVector3(0f, -1f, 0f),
            [new(0f, 0f, 0f), new(1f, 0f, 0f), new(1f, 0f, 1f), new(0f, 0f, 1f)]),
        new((0, 0, 1), new MeshVector3(0f, 0f, 1f),
            [new(1f, 0f, 1f), new(1f, 1f, 1f), new(0f, 1f, 1f), new(0f, 0f, 1f)]),
        new((0, 0, -1), new MeshVector3(0f, 0f, -1f),
            [new(0f, 0f, 0f), new(0f, 1f, 0f), new(1f, 1f, 0f), new(1f, 0f, 0f)]),
    ];

    private static readonly MeshVector2[] QuadUvs =
    [
        new(0f, 0f),
        new(0f, 1f),
        new(1f, 1f),
        new(1f, 0f),
    ];

    public ChunkMeshData Build(ChunkData chunkData, ChunkCoord3 chunkCoord)
    {
        ArgumentNullException.ThrowIfNull(chunkData);

        var positions = new List<MeshVector3>();
        var normals = new List<MeshVector3>();
        var indices = new List<int>();
        var uvs = new List<MeshVector2>();

        var chunkSize = ChunkConstants.ChunkSize;
        var chunkOriginX = chunkCoord.X * chunkSize;
        var chunkOriginY = chunkCoord.Y * chunkSize;
        var chunkOriginZ = chunkCoord.Z * chunkSize;

        for (var lz = 0; lz < chunkSize; lz++)
        {
            for (var ly = 0; ly < chunkSize; ly++)
            {
                for (var lx = 0; lx < chunkSize; lx++)
                {
                    if (chunkData.GetBlock(lx, ly, lz) == 0)
                    {
                        continue;
                    }

                    foreach (var face in FaceOrder)
                    {
                        if (!IsFaceExposed(chunkData, lx, ly, lz, face))
                        {
                            continue;
                        }

                        AppendFace(
                            positions,
                            normals,
                            indices,
                            uvs,
                            face,
                            chunkOriginX + lx,
                            chunkOriginY + ly,
                            chunkOriginZ + lz);
                    }
                }
            }
        }

        return new ChunkMeshData(positions, normals, indices, uvs);
    }

    private static bool IsFaceExposed(ChunkData chunkData, int lx, int ly, int lz, FaceDescriptor face)
    {
        var nx = lx + face.NeighborOffset.X;
        var ny = ly + face.NeighborOffset.Y;
        var nz = lz + face.NeighborOffset.Z;

        if (nx < 0 || nx >= ChunkConstants.ChunkSize ||
            ny < 0 || ny >= ChunkConstants.ChunkSize ||
            nz < 0 || nz >= ChunkConstants.ChunkSize)
        {
            return true;
        }

        return chunkData.GetBlock(nx, ny, nz) == 0;
    }

    private static void AppendFace(
        List<MeshVector3> positions,
        List<MeshVector3> normals,
        List<int> indices,
        List<MeshVector2> uvs,
        FaceDescriptor face,
        long baseX,
        long baseY,
        long baseZ)
    {
        var vertexStart = positions.Count;

        for (var i = 0; i < 4; i++)
        {
            var corner = face.Corners[i];
            positions.Add(new MeshVector3(
                (float)(baseX + corner.X),
                (float)(baseY + corner.Y),
                (float)(baseZ + corner.Z)));
            normals.Add(face.Normal);
            uvs.Add(QuadUvs[i]);
        }

        indices.Add(vertexStart + 0);
        indices.Add(vertexStart + 1);
        indices.Add(vertexStart + 2);
        indices.Add(vertexStart + 0);
        indices.Add(vertexStart + 2);
        indices.Add(vertexStart + 3);
    }

    private readonly record struct FaceDescriptor(
        (int X, int Y, int Z) NeighborOffset,
        MeshVector3 Normal,
        MeshVector3[] Corners);
}
