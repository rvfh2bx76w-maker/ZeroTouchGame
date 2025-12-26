using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
public class GeneratorsTest
{
    [TestCase]
    public void TestTreeGeneration()
    {
        var tree = new TreeGenerator();
        tree._Ready(); // Force generation

        // Verify trunk mesh
        var trunk = tree.GetChild(0) as MeshInstance3D;
        AssertThat(trunk).IsNotNull();
        AssertThat(trunk.Mesh).IsNotNull();
        AssertThat(trunk.Mesh.GetFaces().Length).IsGreater(0);

        // Verify leaves
        if (tree.GetChildCount() > 1)
        {
            var leaves = tree.GetChild(1) as MultiMeshInstance3D;
            AssertThat(leaves).IsNotNull();
            AssertThat(leaves.Multimesh.InstanceCount).IsGreater(0);
        }
    }

    [TestCase]
    public void TestCabinGeneration()
    {
        var cabin = new CabinGenerator();
        cabin._Ready();

        // Check it has children (logs)
        AssertThat(cabin.GetChildCount()).IsGreater(0);
        var root = cabin.GetChild(0);
        AssertThat(root.GetChildCount()).IsGreater(10); // Should have many logs
    }
}
