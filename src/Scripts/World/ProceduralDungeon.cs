using Godot;
using System;

public partial class ProceduralDungeon : Node3D
{
    // A simplified dungeon generator that creates a "Room" using CSGBox3D
    // In a real scenario, this would generate a maze.
    // For this Vertical Slice, it provides a physical space for the "Dungeon" scene.

    public override void _Ready()
    {
        GenerateDungeonRoom();
    }

    private void GenerateDungeonRoom()
    {
        var combiner = new CsgCombiner3D();
        AddChild(combiner);

        // 1. Floor
        var floor = new CsgBox3D();
        floor.Size = new Vector3(20, 1, 20);
        floor.Position = new Vector3(0, -0.5f, 0);
        floor.Material = new StandardMaterial3D() { AlbedoColor = new Color(0.3f, 0.3f, 0.3f) }; // Grey Stone
        combiner.AddChild(floor);

        // 2. Ceiling
        var ceiling = new CsgBox3D();
        ceiling.Size = new Vector3(20, 1, 20);
        ceiling.Position = new Vector3(0, 5.5f, 0);
        ceiling.Material = new StandardMaterial3D() { AlbedoColor = new Color(0.2f, 0.2f, 0.2f) };
        combiner.AddChild(ceiling);

        // 3. Walls
        CreateWall(combiner, new Vector3(0, 2.5f, 10), new Vector3(20, 6, 1)); // Front
        CreateWall(combiner, new Vector3(0, 2.5f, -10), new Vector3(20, 6, 1)); // Back
        CreateWall(combiner, new Vector3(10, 2.5f, 0), new Vector3(1, 6, 20)); // Right
        CreateWall(combiner, new Vector3(-10, 2.5f, 0), new Vector3(1, 6, 20)); // Left

        // 4. Lights (Torches)
        CreateLight(combiner, new Vector3(5, 3, 5));
        CreateLight(combiner, new Vector3(-5, 3, -5));

        // 5. Collision
        // CSGCombiner automatically handles collision if UseCollision is true
        combiner.UseCollision = true;
        combiner.CollisionLayer = 1; // World
    }

    private void CreateWall(Node parent, Vector3 pos, Vector3 size)
    {
        var wall = new CsgBox3D();
        wall.Position = pos;
        wall.Size = size;
        wall.Material = new StandardMaterial3D() { AlbedoColor = new Color(0.4f, 0.2f, 0.1f) }; // Brownish
        parent.AddChild(wall);
    }

    private void CreateLight(Node parent, Vector3 pos)
    {
        var light = new OmniLight3D();
        light.Position = pos;
        light.OmniRange = 10f;
        light.LightColor = new Color(1, 0.6f, 0.2f); // Orange Torchlight
        light.ShadowEnabled = true;
        parent.AddChild(light);
    }
}
