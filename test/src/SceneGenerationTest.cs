using Godot;
using GdUnit4;

[TestSuite]
public class SceneGenerationTest
{
    [TestCase]
    public void GenerateMainScene()
    {
        var builder = new SceneBuilder();
        builder.BuildMainScene();

        // Verify file exists
        Assert.That(FileAccess.FileExists("res://src/Scenes/Main.tscn")).IsTrue();
    }
}
