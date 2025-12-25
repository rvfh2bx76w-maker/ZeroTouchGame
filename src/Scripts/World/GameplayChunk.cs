using Godot;
using System;

public partial class GameplayChunk : Node3D
{
    [Export] public PackedScene WaterfallWonder = null!;
    [Export] public PackedScene TribeOutpost = null!;
    [Export] public PackedScene DungeonPortal = null!;
    [Export] public PackedScene AmbientAudioPocket = null!;

    private Vector2I _coord;
    private int _size;
    private int _seed;

    public void Initialize(Vector2I coord, int size, int worldSeed)
    {
        _coord = coord;
        _size = size;
        _seed = Hash(worldSeed, coord);

        GlobalPosition = new Vector3(_coord.X * _size, 0, _coord.Y * _size);
        Generate();

        // default: no active sim until OverworldStream enables it
        SetActiveSimulation(false);
    }

    public void SetActiveSimulation(bool active)
    {
        // When inactive, we disable process to stop AI ticks/audio logic in this chunk.
        ProcessMode = active ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
        Visible = true;
    }

    private void Generate()
    {
        var rng = new Random(_seed);

        // Vertical-slice deterministic anchors:
        bool isWaterfallChunk = (Math.Abs(_coord.X) % 13 == 0) && (Math.Abs(_coord.Y) % 11 == 0);
        bool isOutpostChunk = (Math.Abs(_coord.X) % 9 == 0) && (Math.Abs(_coord.Y) % 7 == 0);

        if (isWaterfallChunk && WaterfallWonder != null)
            Spawn(WaterfallWonder, rng, "wonder_waterfall");

        if (isOutpostChunk && TribeOutpost != null)
            Spawn(TribeOutpost, rng, "poi_outpost");

        // Portal chance
        double portalChance = isOutpostChunk ? 0.03 : 0.10;
        if (DungeonPortal != null && rng.NextDouble() < portalChance)
            Spawn(DungeonPortal, rng, $"portal_shelter_{_coord.X}_{_coord.Y}");

        // Ambient pocket
        if (AmbientAudioPocket != null && rng.NextDouble() < 0.25)
            Spawn(AmbientAudioPocket, rng, "ambient_pocket");

        // --- Procedural Props (Backfill) ---
        // Spawn 3 to 8 trees per chunk
        int treeCount = rng.Next(3, 9);
        for (int i = 0; i < treeCount; i++)
        {
            var tree = ProceduralProps.CreateTree();
            AddChild(tree);

            float px = (float)rng.NextDouble() * _size;
            float pz = (float)rng.NextDouble() * _size;

            // Note: In a real game we would raycast down to find ground height (Y).
            // For now, assume flat or use noise if we had reference to Terrain generator.
            // Since we don't have easy access to the Terrain noise function here without injecting it,
            // we will spawn them at Y=0. It might look clipped on hills, but acceptable for MVP.
            tree.Position = new Vector3(px, 0, pz);
            tree.Rotation = new Vector3(0, (float)rng.NextDouble() * Mathf.Tau, 0);

            // Random scale variation
            float scale = 0.8f + (float)rng.NextDouble() * 0.4f;
            tree.Scale = new Vector3(scale, scale, scale);
        }
    }

    private void Spawn(PackedScene scene, Random rng, string id)
    {
        var node = scene.Instantiate<Node3D>();
        AddChild(node);

        float px = (float)rng.NextDouble() * _size;
        float pz = (float)rng.NextDouble() * _size;

        node.Position = new Vector3(px, 0, pz);
        node.Name = id;
        node.Rotation = new Vector3(0, (float)rng.NextDouble() * Mathf.Tau, 0);
    }

    private static int Hash(int seed, Vector2I c)
    {
        unchecked
        {
            int h = seed;
            h = (h * 397) ^ c.X;
            h = (h * 397) ^ c.Y;
            return h;
        }
    }
}
