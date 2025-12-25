using Godot;
using System;

public partial class GameRoot : Node
{
    public static GameRoot I { get; private set; } = null!;

    [Export] public int WorldSeed = 123456789;
    public RandomNumberGenerator Rng { get; private set; } = new();

    public override void _EnterTree()
    {
        I = this;
        Rng.Seed = (ulong)WorldSeed;
    }

    public T? FindServiceInGroup<T>(string groupName) where T : class
    {
        foreach (var node in GetTree().GetNodesInGroup(groupName))
            if (node is T t) return t;
        return null;
    }
}
