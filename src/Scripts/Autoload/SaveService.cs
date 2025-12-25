using Godot;
using System;

// Placeholder: wire this to serialize your world state, factions, relationships, properties, inventory, etc.
public partial class SaveService : Node
{
    [Export] public string SavePath = "user://savegame.json";

    public void Save()
    {
        GD.Print("SaveService.Save() - TODO");
    }

    public void Load()
    {
        GD.Print("SaveService.Load() - TODO");
    }
}
