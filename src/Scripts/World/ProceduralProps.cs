using Godot;

public partial class ProceduralProps : Node
{
    // Static helper to generate a simple Tree Mesh
    // Returns a Node3D containing the tree
    public static Node3D CreateTree()
    {
        var treeRoot = new Node3D();

        // Trunk
        var trunkMesh = new CylinderMesh();
        trunkMesh.TopRadius = 0.2f;
        trunkMesh.BottomRadius = 0.4f;
        trunkMesh.Height = 2.0f;

        var trunk = new MeshInstance3D();
        trunk.Mesh = trunkMesh;
        trunk.Position = new Vector3(0, 1.0f, 0);
        trunk.MaterialOverride = new StandardMaterial3D() { AlbedoColor = new Color(0.4f, 0.2f, 0.0f) }; // Brown
        treeRoot.AddChild(trunk);

        // Leaves (Cone)
        var leavesMesh = new CylinderMesh(); // Cone is a cylinder with top radius 0
        leavesMesh.TopRadius = 0.0f;
        leavesMesh.BottomRadius = 1.5f;
        leavesMesh.Height = 3.0f;

        var leaves = new MeshInstance3D();
        leaves.Mesh = leavesMesh;
        leaves.Position = new Vector3(0, 3.0f, 0);
        leaves.MaterialOverride = new StandardMaterial3D() { AlbedoColor = new Color(0.1f, 0.5f, 0.1f) }; // Dark Green
        treeRoot.AddChild(leaves);

        // Collision for trunk
        trunk.CreateTrimeshCollision();
        var staticBody = trunk.GetChild(0) as StaticBody3D;
        if(staticBody != null) staticBody.CollisionLayer = 1;

        return treeRoot;
    }
}
