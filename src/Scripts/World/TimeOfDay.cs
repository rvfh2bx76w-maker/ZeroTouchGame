using Godot;

public partial class TimeOfDay : Node
{
    [Export] public NodePath SunPath = null!;
    [Export] public NodePath WorldEnvironmentPath = null!;

    [Export] public float DayDurationSeconds = 120.0f; // 2 minutes per day
    [Export] public float StartTime = 0.3f; // 0.0 = Midnight, 0.5 = Noon

    private DirectionalLight3D _sun = null!;
    private WorldEnvironment _env = null!;

    public double CurrentTime { get; private set; } // 0.0 to 1.0

    public override void _Ready()
    {
        if (SunPath != null) _sun = GetNodeOrNull<DirectionalLight3D>(SunPath);
        if (WorldEnvironmentPath != null) _env = GetNodeOrNull<WorldEnvironment>(WorldEnvironmentPath);

        if (_sun == null || _env == null)
        {
             GD.PrintErr("TimeOfDay: Missing SunPath or WorldEnvironmentPath.");
             SetProcess(false);
             return;
        }

        CurrentTime = StartTime;
    }

    public override void _Process(double delta)
    {
        // Advance time
        CurrentTime += delta / DayDurationSeconds;
        if (CurrentTime >= 1.0) CurrentTime -= 1.0;

        UpdateSun();
    }

    private void UpdateSun()
    {
        // 0.0 = Midnight (-90 deg)
        // 0.25 = Sunrise (0 deg)
        // 0.5 = Noon (90 deg)
        // 0.75 = Sunset (180 deg)

        // Map 0..1 to -90..270 degrees
        float angle = ((float)CurrentTime * 360.0f) - 90.0f;

        // Rotate sun around X axis
        _sun.RotationDegrees = new Vector3(angle, -30, 0);

        // Disable shadow at night to save perf / avoid weird artifacts
        bool isNight = CurrentTime < 0.2f || CurrentTime > 0.8f;
        _sun.LightEnergy = isNight ? 0.0f : 1.0f;
        _sun.ShadowEnabled = !isNight;
    }
}
