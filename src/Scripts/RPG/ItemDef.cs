using Godot;

[GlobalClass]
public partial class ItemDef : Resource
{
    [Export] public string ItemId = "item_unknown";
    [Export] public string DisplayName = "Unknown";
    [Export(PropertyHint.MultilineText)] public string Description = "";

    [Export] public Texture2D? Icon;
    [Export] public int MaxStack = 20;

    // Combat
    [Export] public int BaseDamage = 0;
    [Export] public float AttackSpeed = 1.0f;

    // Crafting/Survival
    [Export] public bool IsFood = false;
    [Export] public int Nutrition = 0;
}
