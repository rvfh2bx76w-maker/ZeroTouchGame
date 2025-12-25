using System;
using Godot;

/// <summary>
/// CI-friendly headless smoke test runner.
/// Loads the project's configured main scene, lets it run briefly, then exits.
/// Exit code 0 = success, 1 = failure.
/// </summary>
public partial class SmokeTestRunner : Node
{
    // Keep short so CI is fast, long enough for init/procgen to start.
    [Export] public double DurationSeconds { get; set; } = 10.0;

    public override async void _Ready()
    {
        try
        {
            GD.Print("[SmokeTest] Starting…");
            GD.Print($"[SmokeTest] DisplayServer={DisplayServer.GetName()}");

            var mainSceneSetting = ProjectSettings.GetSetting("application/run/main_scene");
            var mainScenePath = mainSceneSetting?.AsString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(mainScenePath))
            {
                GD.PushError("[SmokeTest] application/run/main_scene is not set.");
                GetTree().Quit(1);
                return;
            }

            GD.Print($"[SmokeTest] Loading main scene: {mainScenePath}");
            var packed = GD.Load<PackedScene>(mainScenePath);
            if (packed == null)
            {
                GD.PushError($"[SmokeTest] Failed to load PackedScene: {mainScenePath}");
                GetTree().Quit(1);
                return;
            }

            var inst = packed.Instantiate();
            AddChild(inst);

            // Let the scene run for a bit (initialization, procgen, etc.)
            await ToSignal(GetTree().CreateTimer(DurationSeconds), SceneTreeTimer.SignalName.Timeout);

            GD.Print("[SmokeTest] Completed successfully.");
            GetTree().Quit(0);
        }
        catch (Exception ex)
        {
            GD.PushError("[SmokeTest] Exception:\n" + ex);
            GetTree().Quit(1);
        }
    }
}
