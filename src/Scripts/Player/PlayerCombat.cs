using Godot;

public partial class PlayerCombat : Node
{
    [Export] public NodePath CameraPath = null!;
    [Export] public float LightDamage = 18f;
    [Export] public float HeavyDamage = 30f;

    [Export] public float LightStaminaCost = 12f;
    [Export] public float HeavyStaminaCost = 20f;
    [Export] public float BlockStaminaPerSecond = 8f;

    [Export] public float AttackRange = 2.0f;
    [Export] public float AttackRadius = 0.35f;

    [Export] public float LightWindup = 0.06f;
    [Export] public float LightActive = 0.10f;
    [Export] public float HeavyWindup = 0.12f;
    [Export] public float HeavyActive = 0.12f;

    // Collision mask for enemies (default: layer 3)
    [Export] public uint EnemyCollisionMask = 1u << 2;

    private Camera3D _cam = null!;
    private PlayerStats _stats = null!;
    private bool _blocking;

    public override void _Ready()
    {
        if (CameraPath != null) _cam = GetNodeOrNull<Camera3D>(CameraPath);

        if (_cam == null)
        {
             GD.PrintErr("PlayerCombat: Missing CameraPath.");
        }

        _stats = GetParent().GetNodeOrNull<PlayerStats>("PlayerStats");
        if (_stats == null)
        {
             GD.PrintErr("PlayerCombat: Missing PlayerStats sibling.");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        _blocking = Input.IsActionPressed("block");
        if (_blocking && _stats.Stamina > 0)
            _stats.ConsumeStamina(BlockStaminaPerSecond * (float)delta);

        if (Input.IsActionJustPressed("attack_light"))
            _ = DoAttack(light: true);

        if (Input.IsActionJustPressed("attack_heavy"))
            _ = DoAttack(light: false);

        if (Input.IsActionJustPressed("quickstep"))
            _stats.TryQuickstep();
    }

    private async System.Threading.Tasks.Task DoAttack(bool light)
    {
        if (_blocking) return;

        float cost = light ? LightStaminaCost : HeavyStaminaCost;
        if (!_stats.TryConsumeStamina(cost)) return;

        float windup = light ? LightWindup : HeavyWindup;
        float active = light ? LightActive : HeavyActive;
        float dmg = light ? LightDamage : HeavyDamage;

        await ToSignal(GetTree().CreateTimer(windup), SceneTreeTimer.SignalName.Timeout);
        SweepHit(dmg);
        await ToSignal(GetTree().CreateTimer(active), SceneTreeTimer.SignalName.Timeout);

        _stats.GainSkillXP(light ? SkillType.OneHand : SkillType.TwoHand, light ? 6f : 8f);
    }

    private void SweepHit(float damage)
    {
        var space = GetParent<Node3D>().GetWorld3D().DirectSpaceState;

        Vector3 origin = _cam.GlobalPosition;
        Vector3 forward = -_cam.GlobalTransform.Basis.Z;
        Vector3 to = origin + forward * AttackRange;

        var shape = new CapsuleShape3D { Radius = AttackRadius, Height = 1.0f };

        var query = new PhysicsShapeQueryParameters3D
        {
            Shape = shape,
            Transform = new Transform3D(Basis.Identity, to),
            CollisionMask = EnemyCollisionMask
        };

        var results = space.IntersectShape(query, 8);
        foreach (var hit in results)
        {
            if (!hit.ContainsKey("collider")) continue;
            var collider = hit["collider"].AsGodotObject() as Node;
            if (collider == null) continue;

            // Check for StatsComponent directly or via parent
            var stats = collider as StatsComponent ?? collider.GetParentOrNull<StatsComponent>();
            if (stats != null)
            {
                stats.TakeDamage(damage);
            }
        }
    }
}
