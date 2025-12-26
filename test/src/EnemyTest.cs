using Godot;
using GdUnit4;

[TestSuite]
public class EnemyTest
{
    [TestCase]
    public void TestGoblinStats()
    {
        var generator = new EnemyGenerator();
        // Since GenerateGoblin is called in _Ready, we need to simulate that or call it manually if we could.
        // But in GdUnit, usually we use auto-freeing scene runner.
        // However, Generator creates a child "Goblin" node.

        var runner = ISceneRunner.Visualize(generator);

        // Wait for generation
        var goblin = runner.Scene().GetNode<CharacterBody3D>("Goblin");

        Assertions.AssertThat(goblin).IsNotNull();

        var stats = goblin.GetNode<StatsComponent>("StatsComponent");
        Assertions.AssertThat(stats).IsNotNull();
        Assertions.AssertThat(stats.Health).IsEqual(50);
        Assertions.AssertThat(stats.Speed).IsEqual(120f);
    }
}
