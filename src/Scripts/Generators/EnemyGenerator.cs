using Godot;

public partial class EnemyGenerator : Node3D
{
    public override void _Ready()
    {
        GenerateGoblin();
    }

    private void GenerateGoblin()
    {
        var body = new CharacterBody3D();
        body.Name = "Goblin";
        AddChild(body);

        // 1. Visuals
        var visualRoot = new Node3D();
        body.AddChild(visualRoot);

        var skinMat = new StandardMaterial3D() { AlbedoColor = new Color(0.3f, 0.7f, 0.2f) };
        var loinclothMat = new StandardMaterial3D() { AlbedoColor = new Color(0.3f, 0.2f, 0.1f) };

        CreateBox(visualRoot, new Vector3(0, 0.8f, 0.2f), new Vector3(0.4f, 0.5f, 0.3f), skinMat);
        CreateBox(visualRoot, new Vector3(0, 1.2f, 0.4f), new Vector3(0.3f, 0.3f, 0.35f), skinMat);
        CreateBox(visualRoot, new Vector3(-0.35f, 0.7f, 0.3f), new Vector3(0.15f, 0.6f, 0.15f), skinMat);
        CreateBox(visualRoot, new Vector3(0.35f, 0.7f, 0.3f), new Vector3(0.15f, 0.6f, 0.15f), skinMat);
        CreateBox(visualRoot, new Vector3(-0.15f, 0.3f, 0), new Vector3(0.15f, 0.5f, 0.15f), loinclothMat);
        CreateBox(visualRoot, new Vector3(0.15f, 0.3f, 0), new Vector3(0.15f, 0.5f, 0.15f), loinclothMat);

        var spearShaft = new MeshInstance3D() { Mesh = new CylinderMesh() { TopRadius=0.02f, BottomRadius=0.02f, Height=1.5f }};
        spearShaft.Position = new Vector3(0.35f, 0.8f, 0.3f);
        spearShaft.RotationDegrees = new Vector3(10, 0, 0);
        visualRoot.AddChild(spearShaft);

        var spearTip = new MeshInstance3D() { Mesh = new PrismMesh() { Size = new Vector3(0.1f, 0.3f, 0.05f) }};
        spearTip.Position = new Vector3(0.35f, 1.55f, 0.15f);
        spearTip.MaterialOverride = new StandardMaterial3D() { AlbedoColor = new Color(0.6f, 0.6f, 0.7f) };
        visualRoot.AddChild(spearTip);

        // 2. Physics Collision
        var col = new CollisionShape3D();
        col.Shape = new CapsuleShape3D() { Height = 1.5f, Radius = 0.3f };
        col.Position = new Vector3(0, 0.75f, 0);
        body.AddChild(col);

        // 3. Components
        // Stats
        var stats = new StatsComponent();
        stats.Name = "StatsComponent";
        stats.Health = 50;
        stats.Speed = 120;
        body.AddChild(stats);

        // AI
        var ai = new SimpleEnemyAI();
        body.AddChild(ai);

        // Hitbox
        var hitbox = new Area3D();
        hitbox.Name = "Hitbox";
        hitbox.CollisionLayer = 1 << 2; // Enemy Layer
        hitbox.CollisionMask = 0;

        var hitboxShape = new CollisionShape3D();
        hitboxShape.Shape = new CapsuleShape3D() { Height = 1.6f, Radius = 0.4f };
        hitboxShape.Position = new Vector3(0, 0.8f, 0);
        hitbox.AddChild(hitboxShape);
        body.AddChild(hitbox);
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
