using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class StatBlock : Resource
{
    [Export] public int Level = 1;

    [Export] public int Vitality = 5;
    [Export] public int Endurance = 5;
    [Export] public int Strength = 5;
    [Export] public int Agility = 5;
    [Export] public int Intellect = 5;
    [Export] public int Will = 5;
    [Export] public int Charisma = 5;

    public Dictionary<SkillType, int> SkillLevel = new();
    public Dictionary<SkillType, float> SkillXP = new();

    public int GetSkill(SkillType s) => SkillLevel.TryGetValue(s, out var v) ? v : 1;

    public void AddSkillXP(SkillType s, float xp)
    {
        if (!SkillXP.ContainsKey(s)) SkillXP[s] = 0;
        if (!SkillLevel.ContainsKey(s)) SkillLevel[s] = 1;

        SkillXP[s] += xp;

        float need = 25f + (SkillLevel[s] * SkillLevel[s] * 2.5f);
        while (SkillXP[s] >= need)
        {
            SkillXP[s] -= need;
            SkillLevel[s] += 1;
            need = 25f + (SkillLevel[s] * SkillLevel[s] * 2.5f);
        }
    }

    public int MaxHealth() => 80 + Vitality * 12;
    public int MaxStamina() => 70 + Endurance * 10;
}
