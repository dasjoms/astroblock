using Astroblock.Core.Coords;
using Astroblock.Core.Events;
using Astroblock.Core.Interfaces;
using Astroblock.World;
using Astroblock.World.Generation;
using Astroblock.World.Meshing;
using Astroblock.World.Storage;
using Astroblock.World.Streaming;
using Godot;

namespace Astroblock.Game.GodotHost.Scripts;

public partial class Main : Node3D
{
    [Export] public NodePath PlayerNodePath { get; set; } = new("PlayerAnchor");
    [Export] public NodePath ChunkRootNodePath { get; set; } = new("ChunkRoot");
    [Export] public NodePath DebugLabelNodePath { get; set; } = new("Hud/DebugLabel");
    [Export] public int StreamingRadius { get; set; } = 2;
    [Export] public int WorldSeed { get; set; } = 1337;

    private readonly Dictionary<ChunkCoord3, MeshInstance3D> _chunkInstances = new();
    private readonly List<IDisposable> _subscriptions = new();
    private readonly ChunkMesher _chunkMesher = new();

    private IChunkStore? _chunkStore;
    private WorldCoordinator? _worldCoordinator;
    private InMemoryEventBus? _eventBus;
    private Node3D? _playerNode;
    private Node3D? _chunkRoot;
    private Label? _debugLabel;
    private ChunkCoord3 _currentChunkCoord;

    public override void _Ready()
    {
        _playerNode = GetNodeOrNull<Node3D>(PlayerNodePath) ?? this;
        _chunkRoot = GetNodeOrNull<Node3D>(ChunkRootNodePath) ?? this;
        _debugLabel = GetNodeOrNull<Label>(DebugLabelNodePath);

        _chunkStore = new InMemoryChunkStore();
        _eventBus = new InMemoryEventBus();
        var worldGenerator = new SimpleNoiseWorldGenerator();
        var chunkStreamer = new CubeChunkStreamer();

        _worldCoordinator = new WorldCoordinator(
            chunkStreamer,
            _chunkStore,
            worldGenerator,
            StreamingRadius,
            WorldSeed,
            _eventBus);

        _subscriptions.Add(_eventBus.Subscribe<ChunkLoaded>(OnChunkLoaded));
        _subscriptions.Add(_eventBus.Subscribe<ChunkUnloaded>(OnChunkUnloaded));

        RunWorldTick();
    }

    public override void _Process(double delta)
    {
        RunWorldTick();
    }

    public override void _ExitTree()
    {
        foreach (var subscription in _subscriptions)
        {
            subscription.Dispose();
        }

        _subscriptions.Clear();
    }

    private void RunWorldTick()
    {
        if (_worldCoordinator is null || _chunkStore is null || _playerNode is null)
        {
            return;
        }

        var playerPosition = _playerNode.GlobalPosition;
        var anchor = new WorldAnchor(
            Mathf.FloorToInt(playerPosition.X),
            Mathf.FloorToInt(playerPosition.Y),
            Mathf.FloorToInt(playerPosition.Z));

        _currentChunkCoord = anchor.ToChunkCoord();
        _worldCoordinator.Update(anchor);

        _debugLabel?.SetText($"Loaded chunks: {_chunkInstances.Count} | Current chunk: ({_currentChunkCoord.X}, {_currentChunkCoord.Y}, {_currentChunkCoord.Z})");
    }

    private void OnChunkLoaded(ChunkLoaded chunkLoaded)
    {
        if (_chunkStore is null || _chunkRoot is null)
        {
            return;
        }

        if (_chunkInstances.ContainsKey(chunkLoaded.Coord))
        {
            return;
        }

        if (!_chunkStore.TryGetChunk(chunkLoaded.Coord, out var chunkData))
        {
            return;
        }

        var meshData = _chunkMesher.Build(chunkData, chunkLoaded.Coord);
        if (meshData.Indices.Count == 0)
        {
            return;
        }

        var meshInstance = BuildChunkMeshInstance(meshData, chunkLoaded.Coord);
        _chunkRoot.AddChild(meshInstance);
        _chunkInstances[chunkLoaded.Coord] = meshInstance;
    }

    private void OnChunkUnloaded(ChunkUnloaded chunkUnloaded)
    {
        if (!_chunkInstances.Remove(chunkUnloaded.Coord, out var meshInstance))
        {
            return;
        }

        meshInstance.QueueFree();
    }

    private static MeshInstance3D BuildChunkMeshInstance(ChunkMeshData meshData, ChunkCoord3 coord)
    {
        var positions = new PackedVector3Array();
        var normals = new PackedVector3Array();
        var indices = new PackedInt32Array();

        foreach (var position in meshData.Positions)
        {
            positions.Add(new Vector3(position.X, position.Y, position.Z));
        }

        foreach (var normal in meshData.Normals)
        {
            normals.Add(new Vector3(normal.X, normal.Y, normal.Z));
        }

        foreach (var index in meshData.Indices)
        {
            indices.Add(index);
        }

        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = positions;
        arrays[(int)Mesh.ArrayType.Normal] = normals;
        arrays[(int)Mesh.ArrayType.Index] = indices;

        var mesh = new ArrayMesh();
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        var hue = (Mathf.Abs(coord.X * 17 + coord.Y * 31 + coord.Z * 13) % 360) / 360.0f;
        var material = new StandardMaterial3D
        {
            AlbedoColor = Color.FromHsv(hue, 0.45f, 0.9f),
            Roughness = 1.0f,
            Metallic = 0f,
        };

        mesh.SurfaceSetMaterial(0, material);

        return new MeshInstance3D
        {
            Name = $"Chunk_{coord.X}_{coord.Y}_{coord.Z}",
            Mesh = mesh,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.On,
        };
    }
}
