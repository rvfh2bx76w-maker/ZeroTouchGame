using Godot;

public interface IInteractable
{
    // Return the text to display (e.g., "Open Door", "Talk to Guard")
    string GetInteractionPrompt();

    // Called when the player presses the Interact key
    void Interact(Node interactor);

    // Whether interaction is currently possible
    bool CanInteract(Node interactor);
}
