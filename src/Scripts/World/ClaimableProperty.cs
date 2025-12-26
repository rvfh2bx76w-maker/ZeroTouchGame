using Godot;

public partial class ClaimableProperty : Node3D, IInteractable
{
    [Export] public string PropertyId = "cabin_overlook";
    [Export] public int Cost = 1000;

    public bool IsOwned => PropertySystem.I.IsOwned(PropertyId);

    public string GetInteractionPrompt()
    {
        return IsOwned ? "Manage Property" : $"Claim Property ({Cost} Gold)";
    }

    public void Interact(Node interactor)
    {
        if (!IsOwned)
        {
            // For MVP, just give it to them
            PropertySystem.I.SetOwned(PropertyId, true);
            GD.Print($"Property {PropertyId} claimed!");
        }
        else
        {
            GD.Print("Property already owned.");
        }
    }

    public bool CanInteract(Node interactor)
    {
        return true;
    }
}
