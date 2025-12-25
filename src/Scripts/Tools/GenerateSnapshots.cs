using Godot;
using System.Threading.Tasks;

public partial class GenerateSnapshots : Node
{
    // Run this scene/script to generate the "Validation Snapshots"

    public override async void _Ready()
    {
        var snapper = new Snapshotter();
        AddChild(snapper);

        // 1. Tree
        var tree = new TreeGenerator();
        await snapper.TakeSnapshot(tree, "snapshot_tree.png");
        tree.QueueFree();

        // 2. Cabin
        var cabin = new CabinGenerator();
        await snapper.TakeSnapshot(cabin, "snapshot_cabin.png");
        cabin.QueueFree();

        // 3. Player
        var player = new PlayerGenerator();
        await snapper.TakeSnapshot(player, "snapshot_player.png");
        player.QueueFree();

        // 4. Enemy
        var enemy = new EnemyGenerator();
        await snapper.TakeSnapshot(enemy, "snapshot_enemy.png");
        enemy.QueueFree();

        // 5. Terrain (Small Chunk)
        var terrain = new ProceduralTerrain();
        terrain.Width = 32;
        terrain.Depth = 32;
        await snapper.TakeSnapshot(terrain, "snapshot_terrain.png");
        terrain.QueueFree();

        GD.Print("All snapshots generated in user://snapshots/");
        GetTree().Quit();
    }
}
