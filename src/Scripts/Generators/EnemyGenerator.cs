using Godot;

public partial class EnemyGenerator : Node3D
{
    public override void _Ready()
    {
        GenerateGoblin();
    }

    private void GenerateGoblin()
    {
        var root = new Node3D();
        AddChild(root);

        // Materials
        var skinMat = new StandardMaterial3D() { AlbedoColor = new Color(0.3f, 0.7f, 0.2f) }; // Green
        var loinclothMat = new StandardMaterial3D() { AlbedoColor = new Color(0.3f, 0.2f, 0.1f) };

        // Body (Hunched)
        CreateBox(root, new Vector3(0, 0.8f, 0.2f), new Vector3(0.4f, 0.5f, 0.3f), skinMat); // Torso
        CreateBox(root, new Vector3(0, 1.2f, 0.4f), new Vector3(0.3f, 0.3f, 0.35f), skinMat); // Head (Big ears implied)

        // Arms (Long)
        CreateBox(root, new Vector3(-0.35f, 0.7f, 0.3f), new Vector3(0.15f, 0.6f, 0.15f), skinMat);
        CreateBox(root, new Vector3(0.35f, 0.7f, 0.3f), new Vector3(0.15f, 0.6f, 0.15f), skinMat);

        // Legs (Short)
        CreateBox(root, new Vector3(-0.15f, 0.3f, 0), new Vector3(0.15f, 0.5f, 0.15f), loinclothMat);
        CreateBox(root, new Vector3(0.15f, 0.3f, 0), new Vector3(0.15f, 0.5f, 0.15f), loinclothMat);

        // Gear: Spear
        var spearShaft = new MeshInstance3D() { Mesh = new CylinderMesh() { TopRadius=0.02f, BottomRadius=0.02f, Height=1.5f }};
        spearShaft.Position = new Vector3(0.35f, 0.8f, 0.3f);
        spearShaft.RotationDegrees = new Vector3(10, 0, 0);
        root.AddChild(spearShaft);

        var spearTip = new MeshInstance3D() { Mesh = new PrismMesh() { Size = new Vector3(0.1f, 0.3f, 0.05f) }};
        spearTip.Position = new Vector3(0.35f, 1.55f, 0.15f);
        spearTip.MaterialOverride = new StandardMaterial3D() { AlbedoColor = new Color(0.6f, 0.6f, 0.7f) }; // Metal
        root.AddChild(spearTip);
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
