using Godot;
using System.Collections.Generic;

public partial class PropertySystem : Node
{
    public static PropertySystem I { get; private set; } = null!;

    private readonly HashSet<string> _owned = new();

    public override void _EnterTree()
    {
        I = this;
    }

    public override void _Ready()
    {
        AddToGroup("service_property");
    }

    public bool IsOwned(string propertyId) => _owned.Contains(propertyId);

    public void SetOwned(string propertyId, bool owned)
    {
        if (owned) _owned.Add(propertyId);
        else _owned.Remove(propertyId);
    }

    public HashSet<string> SnapshotOwned() => new(_owned);

    public void LoadOwned(HashSet<string> owned)
    {
        _owned.Clear();
        foreach (var id in owned) _owned.Add(id);
    }
}
