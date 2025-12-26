using Godot;

public partial class ExitDoor : Area3D
{
    [Export] public NodePath SceneFlowPath = null!;
    [Export] public Transform3D OverworldReturnSpawn; // set in editor OR set by script

    private SceneFlow _flow = null!;

    public override void _Ready()
    {
        if (SceneFlowPath != null) _flow = GetNodeOrNull<SceneFlow>(SceneFlowPath);

        if (_flow == null)
        {
             GD.PrintErr("ExitDoor: Missing SceneFlowPath.");
             return;
        }

        BodyEntered += OnBodyEntered;
    }

    private async void OnBodyEntered(Node3D body)
    {
        if (!body.IsInGroup("player")) return;
        await _flow.ExitDungeonAsync(OverworldReturnSpawn);
    }
}
