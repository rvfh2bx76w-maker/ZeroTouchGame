using Godot;

public enum PropertyUnlockMode
{
    Persuasion,
    BossDeed,
    QuestFlag
}

public partial class ClaimableProperty : Node3D
{
    [Export] public string PropertyId = "cabin_overlook";
    [Export] public PropertyUnlockMode UnlockMode = PropertyUnlockMode.Persuasion;

    [Export] public string RequiredFactionId = "tribe_valley";
    [Export] public int RequiredFactionRep = 20;

    [Export] public string RequiredNpcId = "npc_keeper_0";
    [Export] public float RequiredAffinity = 40f;

    [Export] public string RequiredDeedItemId = "deed_cabin_overlook";

    [Export] public NodePath StorageNodePath; // optional
    [Export] public NodePath BedNodePath;     // optional

    public bool IsOwned => PropertySystem.I.IsOwned(PropertyId);

    public bool TryClaimPersuasion()
    {
        if (UnlockMode != PropertyUnlockMode.Persuasion) return false;

        var factions = GameRoot.I.FindServiceInGroup<FactionSystem>("service_faction");
        var rel = GameRoot.I.FindServiceInGroup<RelationshipSystem>("service_relationship");

        if (factions == null || rel == null) return false;

        if (factions.GetRep(RequiredFactionId) < RequiredFactionRep) return false;
        if (rel.GetAffinity(RequiredNpcId) < RequiredAffinity) return false;

        PropertySystem.I.SetOwned(PropertyId, true);
        ApplyOwnershipState();
        return true;
    }

    // Boss deed route: requires InventoryComponent + ItemDef lookup wired by you
    public bool TryClaimWithDeed(InventoryComponent inv, ItemDef deedDef)
    {
        if (UnlockMode != PropertyUnlockMode.BossDeed) return false;

        if (!inv.Remove(deedDef, 1)) return false;

        PropertySystem.I.SetOwned(PropertyId, true);
        ApplyOwnershipState();
        return true;
    }

    public void ApplyOwnershipState()
    {
        var storage = GetNodeOrNull<Node>(StorageNodePath);
        if (storage != null) storage.Set("disabled", !IsOwned);

        var bed = GetNodeOrNull<Node>(BedNodePath);
        if (bed != null) bed.Set("disabled", !IsOwned);
    }
}
