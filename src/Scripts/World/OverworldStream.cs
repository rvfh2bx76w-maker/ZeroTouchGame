using Godot;
using System.Collections.Generic;

public partial class OverworldStream : Node3D
{
    [Export] public NodePath PlayerPath;
    [Export] public NodePath GameplayRootPath;

    [Export] public PackedScene GameplayChunkScene = null!;
    [Export] public int ChunkSize = 128;

    [Export] public int ActiveRadiusChunks = 8;   // ~1.0 km sim radius
    [Export] public int LoadedRadiusChunks = 14;  // ~1.8 km load radius

    private Node3D _player = null!;
    private Node3D _gameplayRoot = null!;
    private readonly Dictionary<Vector2I, GameplayChunk> _chunks = new();

    public override void _Ready()
    {
        _player = GetNode<Node3D>(PlayerPath);
        _gameplayRoot = GetNode<Node3D>(GameplayRootPath);
    }

    public override void _PhysicsProcess(double delta)
    {
        var center = WorldToChunk(_player.GlobalPosition);

        LoadMissing(center);
        UpdateSimulationModes(center);
        UnloadFar(center);
    }

    private Vector2I WorldToChunk(Vector3 pos)
    {
        return new Vector2I(Mathf.FloorToInt(pos.X / ChunkSize), Mathf.FloorToInt(pos.Z / ChunkSize));
    }

    private void LoadMissing(Vector2I center)
    {
        for (int x = -LoadedRadiusChunks; x <= LoadedRadiusChunks; x++)
        for (int z = -LoadedRadiusChunks; z <= LoadedRadiusChunks; z++)
        {
            var c = new Vector2I(center.X + x, center.Y + z);
            if (_chunks.ContainsKey(c)) continue;

            var node = GameplayChunkScene.Instantiate<GameplayChunk>();
            _gameplayRoot.AddChild(node);

            node.Initialize(c, ChunkSize, GameRoot.I.WorldSeed);
            _chunks[c] = node;
        }
    }

    private void UpdateSimulationModes(Vector2I center)
    {
        foreach (var kv in _chunks)
        {
            var c = kv.Key;
            var node = kv.Value;

            int dx = Mathf.Abs(c.X - center.X);
            int dz = Mathf.Abs(c.Y - center.Y);
            bool active = (dx <= ActiveRadiusChunks && dz <= ActiveRadiusChunks);

            node.SetActiveSimulation(active);
        }
    }

    private void UnloadFar(Vector2I center)
    {
        var toRemove = new List<Vector2I>();
        foreach (var kv in _chunks)
        {
            var c = kv.Key;
            int dx = Mathf.Abs(c.X - center.X);
            int dz = Mathf.Abs(c.Y - center.Y);
            if (dx > LoadedRadiusChunks + 1 || dz > LoadedRadiusChunks + 1)
                toRemove.Add(c);
        }

        foreach (var c in toRemove)
        {
            _chunks[c].QueueFree();
            _chunks.Remove(c);
        }
    }
}
