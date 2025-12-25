using Godot;
using System.Collections.Generic;

public enum RelationshipStage
{
    Stranger, Known, Trusted, Romance, Engaged, Married
}

public partial class RelationshipSystem : Node
{
    private readonly Dictionary<string, RelationshipStage> _stage = new();
    private readonly Dictionary<string, float> _affinity = new(); // 0..100

    public override void _Ready()
    {
        AddToGroup("service_relationship");
    }

    public RelationshipStage GetStage(string npcId) =>
        _stage.TryGetValue(npcId, out var s) ? s : RelationshipStage.Stranger;

    public float GetAffinity(string npcId) => _affinity.TryGetValue(npcId, out var a) ? a : 0f;

    public void AddAffinity(string npcId, float delta)
    {
        float next = Mathf.Clamp(GetAffinity(npcId) + delta, 0f, 100f);
        _affinity[npcId] = next;

        var s = GetStage(npcId);
        if (s == RelationshipStage.Stranger && next >= 15) _stage[npcId] = RelationshipStage.Known;
        if (s <= RelationshipStage.Known && next >= 40) _stage[npcId] = RelationshipStage.Trusted;
        if (s <= RelationshipStage.Trusted && next >= 70) _stage[npcId] = RelationshipStage.Romance;
    }

    public bool TryMarry(string npcId)
    {
        if (GetStage(npcId) != RelationshipStage.Romance) return false;
        _stage[npcId] = RelationshipStage.Married;
        return true;
    }

    public (Dictionary<string, string> stage, Dictionary<string, float> affinity) Snapshot()
    {
        var s = new Dictionary<string, string>();
        foreach (var kv in _stage) s[kv.Key] = kv.Value.ToString();
        return (s, new(_affinity));
    }

    public void LoadSnapshot(Dictionary<string, string> stageSnap, Dictionary<string, float> affinitySnap)
    {
        _stage.Clear();
        _affinity.Clear();

        foreach (var kv in stageSnap)
            if (System.Enum.TryParse<RelationshipStage>(kv.Value, out var st))
                _stage[kv.Key] = st;

        foreach (var kv in affinitySnap) _affinity[kv.Key] = kv.Value;
    }
}
