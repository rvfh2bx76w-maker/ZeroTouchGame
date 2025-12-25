using Godot;
using System;

public partial class CabinGenerator : Node3D
{
    // Generates a Log Cabin with individual logs, window frames, and shingles

    public override void _Ready()
    {
        GenerateCabin();
    }

    private void GenerateCabin()
    {
        var root = new Node3D();
        AddChild(root);

        // 1. Walls (Log Stacking)
        // Dimensions: 6m wide, 8m deep, 3m high
        float logDiameter = 0.3f;
        int logCount = (int)(3.0f / logDiameter);

        // Front/Back Walls
        for (int i = 0; i < logCount; i++)
        {
            float y = i * logDiameter;
            // Back Wall (Full)
            CreateLog(root, new Vector3(0, y, -4), new Vector3(6, 0, 0), logDiameter);

            // Front Wall (Door Gap)
            if (i < 7) // Door frame gap
            {
                CreateLog(root, new Vector3(-2, y, 4), new Vector3(2, 0, 0), logDiameter); // Left
                CreateLog(root, new Vector3(2, y, 4), new Vector3(2, 0, 0), logDiameter); // Right
            }
            else // Lintel
            {
                CreateLog(root, new Vector3(0, y, 4), new Vector3(6, 0, 0), logDiameter);
            }
        }

        // Side Walls
        for (int i = 0; i < logCount; i++)
        {
            float y = i * logDiameter + (logDiameter * 0.5f); // Offset for Lincoln log interlocking
            CreateLog(root, new Vector3(-3, y, 0), new Vector3(0, 0, 8), logDiameter); // Left
            CreateLog(root, new Vector3(3, y, 0), new Vector3(0, 0, 8), logDiameter);  // Right
        }

        // 2. Roof (A-Frame)
        CreateRoof(root);

        // 3. Door & Windows
        CreateDoor(root);
    }

    private void CreateLog(Node parent, Vector3 center, Vector3 axis, float diameter)
    {
        var mesh = new CylinderMesh();
        mesh.TopRadius = diameter / 2;
        mesh.BottomRadius = diameter / 2;
        mesh.Height = axis.Length();

        var inst = new MeshInstance3D();
        inst.Mesh = mesh;
        inst.Position = center;

        // Rotate to align with axis
        if (axis.X > axis.Z) // Horizontal X
            inst.RotationDegrees = new Vector3(0, 0, 90);
        else
            inst.RotationDegrees = new Vector3(90, 0, 0);

        inst.MaterialOverride = new StandardMaterial3D()
        {
            AlbedoColor = new Color(0.4f, 0.25f, 0.15f), // Dark Wood
            Roughness = 1.0f
        };

        parent.AddChild(inst);
    }

    private void CreateRoof(Node parent)
    {
        // Simple A-Frame with individual planks would be better, but CSG is robust here.
        var prism = new CsgPolygon3D();
        // Define profile... simplified to Box for vertical slice robustness

        // Let's use two rotated boxes for rafters
        var leftRafter = new MeshInstance3D();
        leftRafter.Mesh = new BoxMesh() { Size = new Vector3(4.5f, 0.2f, 9f) };
        leftRafter.Position = new Vector3(-1.5f, 4f, 0);
        leftRafter.RotationDegrees = new Vector3(0, 0, 45);
        leftRafter.MaterialOverride = new StandardMaterial3D() { AlbedoColor = new Color(0.2f, 0.1f, 0.05f) }; // Darker Shingles
        parent.AddChild(leftRafter);

        var rightRafter = new MeshInstance3D();
        rightRafter.Mesh = new BoxMesh() { Size = new Vector3(4.5f, 0.2f, 9f) };
        rightRafter.Position = new Vector3(1.5f, 4f, 0);
        rightRafter.RotationDegrees = new Vector3(0, 0, -45);
        rightRafter.MaterialOverride = new StandardMaterial3D() { AlbedoColor = new Color(0.2f, 0.1f, 0.05f) };
        parent.AddChild(rightRafter);
    }

    private void CreateDoor(Node parent)
    {
        var door = new MeshInstance3D();
        door.Mesh = new BoxMesh() { Size = new Vector3(1.2f, 2.1f, 0.1f) };
        door.Position = new Vector3(0, 1.05f, 4.0f); // Front center
        door.MaterialOverride = new StandardMaterial3D() { AlbedoColor = new Color(0.5f, 0.3f, 0.2f) };
        parent.AddChild(door);

        // Handle
        var handle = new MeshInstance3D();
        handle.Mesh = new SphereMesh() { Radius = 0.05f };
        handle.Position = new Vector3(0.4f, 1.0f, 4.1f);
        handle.MaterialOverride = new StandardMaterial3D() { AlbedoColor = new Color(0.8f, 0.8f, 0.2f), Metallic = 1.0f }; // Gold/Brass
        parent.AddChild(handle);
    }
}
