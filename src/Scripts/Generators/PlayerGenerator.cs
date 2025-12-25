using Godot;

public partial class PlayerGenerator : Node3D
{
    public override void _Ready()
    {
        GeneratePlayer();
    }

    private void GeneratePlayer()
    {
        var root = new Node3D();
        AddChild(root);

        // Materials
        var skinMat = new StandardMaterial3D() { AlbedoColor = new Color(0.9f, 0.7f, 0.6f) };
        var tunicMat = new StandardMaterial3D() { AlbedoColor = new Color(0.4f, 0.3f, 0.2f) }; // Leather
        var pantsMat = new StandardMaterial3D() { AlbedoColor = new Color(0.2f, 0.2f, 0.3f) }; // Blue-ish rags

        // Body Parts
        CreateBox(root, new Vector3(0, 1.5f, 0), new Vector3(0.5f, 0.7f, 0.3f), tunicMat); // Torso
        CreateBox(root, new Vector3(0, 1.95f, 0), new Vector3(0.25f, 0.3f, 0.25f), skinMat); // Head

        // Arms
        CreateBox(root, new Vector3(-0.4f, 1.5f, 0), new Vector3(0.2f, 0.6f, 0.2f), skinMat); // Left Arm
        CreateBox(root, new Vector3(0.4f, 1.5f, 0), new Vector3(0.2f, 0.6f, 0.2f), skinMat); // Right Arm

        // Legs
        CreateBox(root, new Vector3(-0.15f, 0.5f, 0), new Vector3(0.2f, 0.8f, 0.2f), pantsMat); // Left Leg
        CreateBox(root, new Vector3(0.15f, 0.5f, 0), new Vector3(0.2f, 0.8f, 0.2f), pantsMat); // Right Leg

        // Gear: Stone Axe in Right Hand
        var axeHandle = new MeshInstance3D() { Mesh = new CylinderMesh() { TopRadius=0.03f, BottomRadius=0.03f, Height=0.8f }};
        axeHandle.Position = new Vector3(0.4f, 1.2f, 0.3f);
        axeHandle.RotationDegrees = new Vector3(45, 0, 0);
        root.AddChild(axeHandle);

        var axeHead = new MeshInstance3D() { Mesh = new BoxMesh() { Size = new Vector3(0.1f, 0.3f, 0.3f) }};
        axeHead.Position = new Vector3(0.4f, 1.5f, 0.6f);
        axeHead.MaterialOverride = new StandardMaterial3D() { AlbedoColor = new Color(0.5f, 0.5f, 0.5f) }; // Stone
        root.AddChild(axeHead);
    }

    private void CreateBox(Node parent, Vector3 pos, Vector3 size, Material mat)
    {
        var m = new MeshInstance3D();
        m.Mesh = new BoxMesh() { Size = size };
        m.Position = pos;
        m.MaterialOverride = mat;
        parent.AddChild(m);
    }
}
