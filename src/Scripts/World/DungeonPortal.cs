using Godot;

public partial class DungeonPortal : Area3D
{
    [Export] public string DungeonId = "shelter_0";
    [Export] public PackedScene DungeonInteriorScene = null!;
    [Export] public NodePath SceneFlowPath = null!;
    [Export] public NodePath PlayerReturnMarkerPath = null!; // Marker3D near the overworld door

    private SceneFlow _flow = null!;
    private Node3D _returnMarker = null!;

    public override void _Ready()
    {
        if (SceneFlowPath != null) _flow = GetNodeOrNull<SceneFlow>(SceneFlowPath);
        if (PlayerReturnMarkerPath != null) _returnMarker = GetNodeOrNull<Node3D>(PlayerReturnMarkerPath);

        if (_flow == null || _returnMarker == null)
        {
             GD.PrintErr("DungeonPortal: Missing SceneFlowPath or PlayerReturnMarkerPath.");
             // Disabling collision to prevent issues
             Monitorable = false;
             Monitoring = false;
             return;
        }

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
