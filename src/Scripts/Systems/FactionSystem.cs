using Godot;
using System.Collections.Generic;

public partial class FactionSystem : Node
{
    private readonly Dictionary<string, int> _rep = new();

    public override void _Ready()
    {
        AddToGroup("service_faction");
    }

    public int GetRep(string factionId) => _rep.TryGetValue(factionId, out var v) ? v : 0;

    public void AddRep(string factionId, int delta)
    {
        int cur = GetRep(factionId);
        int next = Mathf.Clamp(cur + delta, -100, 100);
        _rep[factionId] = next;

        EventBus.I?.FactionReputationChanged(factionId);
    }

    public Dictionary<string, int> Snapshot() => new(_rep);

    public void LoadSnapshot(Dictionary<string, int> snap)
    {
        _rep.Clear();
        foreach (var kv in snap) _rep[kv.Key] = kv.Value;
    }
}
