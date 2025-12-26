using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
public class SceneGenerationTest
{
    [TestCase]
    public void GenerateMainScene()
    {
        var builder = new SceneBuilder();
        builder.BuildMainScene();

        // Verify file exists
        AssertThat(FileAccess.FileExists("res://src/Scenes/Main.tscn")).IsTrue();
    }
}
