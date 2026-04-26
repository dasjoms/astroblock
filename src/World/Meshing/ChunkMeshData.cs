namespace Astroblock.World.Meshing;

/// <summary>
/// Engine-agnostic mesh payload generated from chunk voxel data.
/// </summary>
public sealed class ChunkMeshData
{
    public ChunkMeshData(
        IReadOnlyList<MeshVector3> positions,
        IReadOnlyList<MeshVector3> normals,
        IReadOnlyList<int> indices,
        IReadOnlyList<MeshVector2> uvs)
    {
        Positions = positions;
        Normals = normals;
        Indices = indices;
        Uvs = uvs;
    }

    public IReadOnlyList<MeshVector3> Positions { get; }

    public IReadOnlyList<MeshVector3> Normals { get; }

    public IReadOnlyList<int> Indices { get; }

    public IReadOnlyList<MeshVector2> Uvs { get; }
}

public readonly record struct MeshVector3(float X, float Y, float Z);

public readonly record struct MeshVector2(float U, float V);
