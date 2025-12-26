using Godot;

public partial class SimplePlayerController : CharacterBody3D
{
    [Export] public NodePath HeadPath = null!;
    [Export] public float WalkSpeed = 5.0f;
    [Export] public float SprintSpeed = 7.5f;
    [Export] public float MouseSensitivity = 0.002f;

    private Node3D _head = null!;
    private float _pitch;

    public override void _Ready()
    {
        AddToGroup("player");
        if (HeadPath != null)
            _head = GetNodeOrNull<Node3D>(HeadPath);

        if (_head == null)
        {
            // Fallback: Create a head if missing
            _head = new Node3D();
            _head.Name = "Head";
            _head.Position = new Vector3(0, 1.7f, 0);
            AddChild(_head);

            // If camera is not child of head, it might be an issue, but we assume Camera is attached to Head in Scene or we need to spawn it.
            // SimplePlayerController assumes the scene structure has a Head.
        }

        Input.MouseMode = Input.MouseModeEnum.Captured;

        // Visuals: Procedurally generate the player mesh
        var visual = new PlayerGenerator();
        AddChild(visual);
        // visual.Position = Vector3.Zero; // Default is fine
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e is InputEventMouseMotion mm)
        {
            RotateY(-mm.Relative.X * MouseSensitivity);
            if (_head != null)
            {
                _pitch = Mathf.Clamp(_pitch - mm.Relative.Y * MouseSensitivity, -1.2f, 1.2f);
                _head.Rotation = new Vector3(_pitch, 0, 0);
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        var input = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        var dir = (Transform.Basis * new Vector3(input.X, 0, input.Y)).Normalized();

        float speed = Input.IsActionPressed("sprint") ? SprintSpeed : WalkSpeed;
        Velocity = new Vector3(dir.X * speed, Velocity.Y, dir.Z * speed);

        if (!IsOnFloor())
            Velocity += Vector3.Down * 18f * (float)delta;

        MoveAndSlide();
    }
}
