using Godot;
using System.Collections.Generic;

public partial class DialogueUI : Control
{
    [Signal] public delegate void OptionSelectedEventHandler(int index);

    private Panel _panel = null!;
    private Label _npcLabel = null!;
    private VBoxContainer _optionsContainer = null!;
    private List<Button> _optionButtons = new List<Button>();

    public override void _Ready()
    {
        // 1. Create Panel (Bottom 1/3 of screen)
        _panel = new Panel();
        _panel.AnchorsPreset = (int)LayoutPreset.BottomWide;
        _panel.SetAnchor(Side.Top, 0.7f);
        _panel.SetAnchor(Side.Bottom, 1.0f);
        _panel.GrowHorizontal = GrowDirection.Both;
        _panel.GrowVertical = GrowDirection.Begin;
        _panel.Visible = false;
        AddChild(_panel);

        // 2. NPC Text
        _npcLabel = new Label();
        _npcLabel.SetAnchor(Side.Left, 0.05f);
        _npcLabel.SetAnchor(Side.Top, 0.1f);
        _npcLabel.SetAnchor(Side.Right, 0.95f);
        _npcLabel.SetAnchor(Side.Bottom, 0.4f);
        _npcLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _panel.AddChild(_npcLabel);

        // 3. Options Container
        _optionsContainer = new VBoxContainer();
        _optionsContainer.SetAnchor(Side.Left, 0.05f);
        _optionsContainer.SetAnchor(Side.Top, 0.5f);
        _optionsContainer.SetAnchor(Side.Right, 0.95f);
        _optionsContainer.SetAnchor(Side.Bottom, 0.95f);
        _panel.AddChild(_optionsContainer);
    }

    // Public API to open dialogue
    public void Open(string npcText, string[] options)
    {
        _panel.Visible = true;
        _npcLabel.Text = npcText;

        // Clear old buttons
        foreach(var btn in _optionButtons) btn.QueueFree();
        _optionButtons.Clear();

        Input.MouseMode = Input.MouseModeEnum.Visible;

        for (int i = 0; i < options.Length; i++)
        {
            var btn = new Button();
            btn.Text = options[i];
            int idx = i;
            btn.Pressed += () => OnOptionSelected(idx);
            _optionsContainer.AddChild(btn);
            _optionButtons.Add(btn);
        }
    }

    private void OnOptionSelected(int index)
    {
        _panel.Visible = false;
        Input.MouseMode = Input.MouseModeEnum.Captured;

        EmitSignal(SignalName.OptionSelected, index);
    }
}
