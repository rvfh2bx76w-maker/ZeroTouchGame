using Godot;

public partial class InteractionHUD : Control
{
    private Label _promptLabel;
    private ColorRect _crosshair;

    public override void _Ready()
    {
        // Setup Crosshair (Center of screen)
        _crosshair = new ColorRect();
        _crosshair.Color = new Color(1, 1, 1, 0.5f);
        _crosshair.SetSize(new Vector2(4, 4));
        _crosshair.Position = (GetViewportRect().Size / 2) - new Vector2(2, 2);
        // Anchor to center
        _crosshair.AnchorsPreset = (int)LayoutPreset.Center;
        AddChild(_crosshair);

        // Setup Label (Below crosshair)
        _promptLabel = new Label();
        _promptLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _promptLabel.Position = (GetViewportRect().Size / 2) + new Vector2(0, 20);
        _promptLabel.AnchorsPreset = (int)LayoutPreset.CenterBottom;
        _promptLabel.Text = "";
        AddChild(_promptLabel);
    }

    public void ShowPrompt(string text)
    {
        _promptLabel.Text = text;
        _promptLabel.Visible = true;
    }

    public void HidePrompt()
    {
        _promptLabel.Visible = false;
    }
}
