using Godot;
using System.Threading.Tasks;

public partial class Snapshotter : Node3D
{
    private SubViewport _viewport = null!;
    private Camera3D _camera = null!;
    private Node3D _subjectContainer = null!;
    private DirectionalLight3D _light = null!;
    private WorldEnvironment _env = null!;

    public override void _Ready()
    {
        // Setup a headless viewport for rendering
        _viewport = new SubViewport();
        _viewport.Size = new Vector2I(1920, 1080);
        _viewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Once; // We will trigger it manually
        AddChild(_viewport);

        // Scene container
        var sceneRoot = new Node3D();
        _viewport.AddChild(sceneRoot);

        // Camera
        _camera = new Camera3D();
        _camera.Position = new Vector3(0, 2, 5);
        _camera.LookAt(new Vector3(0, 1, 0));
        sceneRoot.AddChild(_camera);

        // Light
        _light = new DirectionalLight3D();
        _light.RotationDegrees = new Vector3(-45, 45, 0);
        _light.ShadowEnabled = true;
        sceneRoot.AddChild(_light);

        // Environment (Neutral Grey)
        _env = new WorldEnvironment();
        var envRes = new Godot.Environment();
        envRes.BackgroundMode = Godot.Environment.BGMode.Color;
        envRes.BackgroundColor = new Color(0.2f, 0.2f, 0.2f); // Dark Grey Studio Background
        envRes.AmbientLightSource = Godot.Environment.AmbientSource.Color;
        envRes.AmbientLightColor = new Color(1, 1, 1);
        envRes.AmbientLightEnergy = 0.2f;
        _env.Environment = envRes;
        sceneRoot.AddChild(_env);

        _subjectContainer = new Node3D();
        sceneRoot.AddChild(_subjectContainer);
    }

    public async Task TakeSnapshot(Node3D subject, string filename)
    {
        // 1. Clear previous subject
        foreach (Node child in _subjectContainer.GetChildren())
            child.QueueFree();

        // 2. Add new subject
        _subjectContainer.AddChild(subject);
        subject.Position = Vector3.Zero;

        // 3. Auto-Frame Camera
        // (Simple bounding box approximation or fixed position for now)
        _camera.Position = new Vector3(0, 2, 4);
        _camera.LookAt(new Vector3(0, 1, 0));

        // 4. Render
        _viewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;

        // Wait 2 frames for render
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        // 5. Capture
        var img = _viewport.GetTexture().GetImage();
        string path = $"user://snapshots/{filename}";

        // Ensure directory exists
        if (!DirAccess.DirExistsAbsolute("user://snapshots"))
            DirAccess.MakeDirAbsolute("user://snapshots");

        img.SavePng(path);
        GD.Print($"Snapshot saved to {path}");
    }
}
