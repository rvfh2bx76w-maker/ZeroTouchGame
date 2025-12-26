using Godot;
using System;
using System.Threading.Tasks;

public partial class SceneFlow : Node
{
    [Export] public NodePath OverworldRootPath = null!;
    [Export] public NodePath DungeonRootPath = null!;
    [Export] public NodePath FadeOverlayPath = null!; // ColorRect
    [Export] public float FadeSeconds = 0.25f;

    private Node3D _overworldRoot = null!;
    private Node3D _dungeonRoot = null!;
    private ColorRect _fade = null!;
    private Node3D? _activeDungeon;

    public override void _Ready()
    {
        if (OverworldRootPath != null) _overworldRoot = GetNodeOrNull<Node3D>(OverworldRootPath);
        if (DungeonRootPath != null) _dungeonRoot = GetNodeOrNull<Node3D>(DungeonRootPath);
        if (FadeOverlayPath != null) _fade = GetNodeOrNull<ColorRect>(FadeOverlayPath);

        if (_overworldRoot == null || _dungeonRoot == null || _fade == null)
        {
            GD.PrintErr("SceneFlow: Missing required NodePaths.");
            SetProcess(false);
            return;
        }

        _fade.Modulate = new Color(0, 0, 0, 0);
        _fade.Visible = true;
    }

    public async Task EnterDungeonAsync(PackedScene dungeonScene, Transform3D dungeonRootTransform)
    {
        await FadeTo(1f);

        _overworldRoot.Visible = false;
        _overworldRoot.ProcessMode = ProcessModeEnum.Disabled;

        // Show dungeon root
        _dungeonRoot.Visible = true;
        _dungeonRoot.ProcessMode = ProcessModeEnum.Inherit;

        if (_activeDungeon != null && IsInstanceValid(_activeDungeon))
            _activeDungeon.QueueFree();

        _activeDungeon = dungeonScene.Instantiate<Node3D>();
        _dungeonRoot.AddChild(_activeDungeon);
        _activeDungeon.GlobalTransform = dungeonRootTransform;

        await FadeTo(0f);
    }

    public async Task ExitDungeonAsync(Transform3D playerReturnSpawn)
    {
        await FadeTo(1f);

        if (_activeDungeon != null && IsInstanceValid(_activeDungeon))
            _activeDungeon.QueueFree();
        _activeDungeon = null;

        _overworldRoot.Visible = true;
        _overworldRoot.ProcessMode = ProcessModeEnum.Inherit;

        // Hide dungeon root
        _dungeonRoot.Visible = false;
        _dungeonRoot.ProcessMode = ProcessModeEnum.Disabled;

        var player = GetTree().GetFirstNodeInGroup("player") as Node3D;
        if (player != null) player.GlobalTransform = playerReturnSpawn;

        await FadeTo(0f);
    }

    private async Task FadeTo(float alpha)
    {
        var tween = CreateTween();
        tween.TweenProperty(_fade, "modulate:a", alpha, FadeSeconds);
        await ToSignal(tween, Tween.SignalName.Finished);
    }
}
