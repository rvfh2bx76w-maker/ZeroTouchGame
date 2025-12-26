using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
public class EnemyTest
{
    [TestCase]
    public void TestGoblinStats()
    {
        // 1. Arrange: Construct the Goblin entity manually
        var goblin = new CharacterBody3D { Name = "Goblin" };

        // Add StatsComponent
        var stats = new StatsComponent();
        stats.Name = "StatsComponent";
        stats.Health = 50f;
        stats.Speed = 120f;
        stats.AggroRange = 300f;
        goblin.AddChild(stats);

        // Add SimpleEnemyAI (Behavior)
        var ai = new SimpleEnemyAI();
        ai.Name = "SimpleEnemyAI";
        goblin.AddChild(ai);

        // Simulate _Ready by calling it manually or adding to tree (but no tree in headless w/o runner)
        // For unit testing logic, we check values directly.

        // 2. Act (nothing to act on, just verification of setup)

        // 3. Assert
        AssertThat(stats.Health).IsEqual(50f);
        AssertThat(stats.Speed).IsEqual(120f);
        AssertThat(stats.AggroRange).IsEqual(300f);

        // Optional: verify AI can find stats
        // We can't easily test _Ready/GetNode without adding to a SceneTree,
        // but we can verify the structure we built matches expectations.
        AssertThat(goblin.GetNode<StatsComponent>("StatsComponent")).IsNotNull();

        // Cleanup
        goblin.Free();
    }
}
