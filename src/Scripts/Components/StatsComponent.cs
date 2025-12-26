using Godot;

public partial class StatsComponent : Node
{
    [Export] public float Health = 100f;
    [Export] public float Speed = 5.0f;

    public void TakeDamage(float amount)
    {
        Health -= amount;
        if (Health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GetParent().QueueFree();
    }
}
