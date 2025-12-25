using Godot;

[Tool]
public partial class SceneGeneratorRunner : EditorScript
{
    public override void _Run()
    {
        var builder = new SceneBuilder();
        builder.BuildMainScene();
        GD.Print("Scene generation complete via EditorScript.");
    }
}
