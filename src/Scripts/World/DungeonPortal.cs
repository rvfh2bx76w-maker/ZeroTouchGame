using Godot;

public partial class DungeonPortal : Area3D
{
    [Export] public string DungeonId = "shelter_0";
    [Export] public PackedScene DungeonInteriorScene = null!;
    [Export] public NodePath SceneFlowPath;
    [Export] public NodePath PlayerReturnMarkerPath; // Marker3D near the overworld door

    private SceneFlow _flow = null!;
    private Node3D _returnMarker = null!;

    public override void _Ready()
    {
        _flow = GetNode<SceneFlow>(SceneFlowPath);
        _returnMarker = GetNode<Node3D>(PlayerReturnMarkerPath);
        BodyEntered += OnBodyEntered;
    }

    private async void OnBodyEntered(Node3D body)
    {
        if (!body.IsInGroup("player")) return;
        if (DungeonInteriorScene == null) return;

        // Spawn dungeon root at identity; place an interior Marker3D for player spawn once you add it.
        await _flow.EnterDungeonAsync(DungeonInteriorScene, new Transform3D(Basis.Identity, Vector3.Zero));
    }

    public Transform3D GetReturnSpawn() => _returnMarker.GlobalTransform;
}
