using Godot;

public partial class CombatComponent : Node
{
    [Export] public float MaxHealth = 100f;
    [Export] public float Armor = 0f;
    [Export] public float MaxPosture = 60f;
    [Export] public float PostureRegenPerSec = 10f;

    public float Health { get; private set; }
    public float Posture { get; private set; }

    public override void _Ready()
    {
        Health = MaxHealth;
        Posture = MaxPosture;
    }

    public override void _PhysicsProcess(double delta)
    {
        Posture = Mathf.Min(MaxPosture, Posture + PostureRegenPerSec * (float)delta);
    }

    public void ApplyDamage(float raw, Node? source = null)
    {
        float mitigated = Mathf.Max(1f, raw - Armor);
        Health -= mitigated;

        Posture -= mitigated * 0.8f;
        if (Posture <= 0)
        {
            Posture = MaxPosture * 0.5f;
            // TODO: trigger stagger anim/knockback on parent
        }

        if (Health <= 0)
        {
            // TODO: death handler (loot drops, faction rep changes, etc.)
            GetParent().QueueFree();
        }
    }
}
