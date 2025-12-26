using Godot;

public partial class PlayerInteraction : Node
{
    [Export] public float InteractionDistance = 3.0f;
    [Export] public NodePath CameraPath;
    [Export] public NodePath HUDPath;

    // Collision mask for Interactables (Layer 4)
    [Export] public uint InteractionMask = 1u << 3; // Layer 4 (0-indexed 3)

    private Camera3D _camera = null!;
    private InteractionHUD _hud = null!;
    private IInteractable? _currentInteractable;

    public override void _Ready()
    {
        _camera = GetNode<Camera3D>(CameraPath);
        _hud = GetNode<InteractionHUD>(HUDPath);
    }

    public override void _PhysicsProcess(double delta)
    {
        CheckForInteractable();

        if (Input.IsActionJustPressed("interact") && _currentInteractable != null)
        {
            if (_currentInteractable.CanInteract(GetParent()))
            {
                _currentInteractable.Interact(GetParent());
            }
        }
    }

    private void CheckForInteractable()
    {
        var spaceState = _camera.GetWorld3D().DirectSpaceState;
        var from = _camera.GlobalPosition;
        var to = from - _camera.GlobalTransform.Basis.Z * InteractionDistance;

        var query = PhysicsRayQueryParameters3D.Create(from, to, InteractionMask);
        query.CollideWithAreas = true;
        query.CollideWithBodies = true;

        var result = spaceState.IntersectRay(query);

        if (result.Count > 0)
        {
            var collider = result["collider"].AsGodotObject() as Node;

            // Check if the collider or its parent implements IInteractable
            // (We check parent because the collider might be a StaticBody child of the logic node)
            var interactable = GetInteractable(collider);

            if (interactable != null && interactable.CanInteract(GetParent()))
            {
                if (_currentInteractable != interactable)
                {
                    _currentInteractable = interactable;
                    _hud.ShowPrompt(_currentInteractable.GetInteractionPrompt());
                }
                return;
            }
        }

        // Nothing found
        if (_currentInteractable != null)
        {
            _currentInteractable = null;
            _hud.HidePrompt();
        }
    }

    private IInteractable? GetInteractable(Node node)
    {
        if (node is IInteractable i) return i;
        if (node.GetParent() is IInteractable p) return p;
        return null;
    }
}
