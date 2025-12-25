using Godot;
using System;

public partial class EventBus : Node
{
    public static EventBus I { get; private set; } = null!;

    public override void _EnterTree() => I = this;

    public event Action<int>? OnSkillGainedXP;
    public event Action<string>? OnEnteredDungeon;
    public event Action<string>? OnFactionReputationChanged;

    public void SkillGainedXP(int amount) => OnSkillGainedXP?.Invoke(amount);
    public void EnteredDungeon(string dungeonId) => OnEnteredDungeon?.Invoke(dungeonId);
    public void FactionReputationChanged(string factionId) => OnFactionReputationChanged?.Invoke(factionId);
}
