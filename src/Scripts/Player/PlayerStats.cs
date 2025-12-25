using Godot;

public partial class PlayerStats : Node
{
    [Export] public float MaxHealth = 120f;
    [Export] public float MaxStamina = 100f;
    [Export] public float StaminaRegenPerSec = 22f;
    [Export] public float RegenDelayAfterSpend = 0.35f;

    [Export] public float QuickstepCost = 18f;
    [Export] public float QuickstepDistance = 3.0f;
    [Export] public float QuickstepSeconds = 0.08f;

    public float Health { get; private set; }
    public float Stamina { get; private set; }

    private float _regenBlocked;

    [Export] public StatBlock? StatBlock; // optional Resource

    public override void _Ready()
    {
        Health = MaxHealth;
        Stamina = MaxStamina;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_regenBlocked > 0f)
        {
            _regenBlocked -= (float)delta;
            return;
        }

        Stamina = Mathf.Min(MaxStamina, Stamina + StaminaRegenPerSec * (float)delta);
    }

    public bool TryConsumeStamina(float amount)
    {
        if (Stamina < amount) return false;
        ConsumeStamina(amount);
        return true;
    }

    public void ConsumeStamina(float amount)
    {
        Stamina = Mathf.Max(0f, Stamina - amount);
        _regenBlocked = RegenDelayAfterSpend;
    }

    public void GainSkillXP(SkillType skill, float xp)
    {
        if (StatBlock == null) return;
        StatBlock.AddSkillXP(skill, xp);
    }

    public async void TryQuickstep()
    {
        if (!TryConsumeStamina(QuickstepCost)) return;

        var player = GetParent() as CharacterBody3D;
        if (player == null) return;

        // Quickstep forward; replace with input-direction dash later
        Vector3 forward = -player.GlobalTransform.Basis.Z;
        Vector3 start = player.GlobalPosition;
        Vector3 end = start + forward * QuickstepDistance;

        var tween = CreateTween();
        tween.TweenProperty(player, "global_position", end, QuickstepSeconds);
        await ToSignal(tween, Tween.SignalName.Finished);

        GainSkillXP(SkillType.Dodging, 3f);
    }
}
