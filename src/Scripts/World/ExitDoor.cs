using Godot;

public partial class ExitDoor : Area3D
{
    [Export] public NodePath SceneFlowPath;
    [Export] public Transform3D OverworldReturnSpawn; // set in editor OR set by script

    private SceneFlow _flow = null!;

    public override void _Ready()
    {
        _flow = GetNode<SceneFlow>(SceneFlowPath);
        BodyEntered += OnBodyEntered;
    }

    private async void OnBodyEntered(Node3D body)
    {
        if (!body.IsInGroup("player")) return;
        await _flow.ExitDungeonAsync(OverworldReturnSpawn);
    }
}
