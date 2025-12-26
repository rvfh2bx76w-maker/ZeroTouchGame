using Godot;
using System;
using System.Collections.Generic;

public partial class TreeGenerator : Node3D
{
    [Export] public int Seed = 1234;
    [Export] public int Height = 8;

    private Random _rng;
    private List<Transform3D> _leafTransforms = new List<Transform3D>();

    public override void _Ready()
    {
        _rng = new Random(Seed);
        Generate();
    }

    public void Generate()
    {
        _leafTransforms.Clear();

        // 1. Trunk & Branches (Mesh Generation)
        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);

        // Brown Bark Material
        var barkMat = new StandardMaterial3D();
        barkMat.AlbedoColor = new Color(0.35f, 0.2f, 0.1f);
        barkMat.Roughness = 0.9f;
        st.SetMaterial(barkMat);

        // Recursive generation
        BuildBranch(st, Vector3.Zero, Vector3.Up, length: 4.0f, radius: 0.5f, depth: 4);

        st.GenerateNormals();
        st.GenerateTangents();

        var trunkMesh = st.Commit();
        var trunkInstance = new MeshInstance3D();
        trunkInstance.Mesh = trunkMesh;
        trunkInstance.CreateTrimeshCollision(); // Add collision to trunk
        AddChild(trunkInstance);

        // 2. Leaves (MultiMesh)
        if (_leafTransforms.Count > 0)
        {
            var multiMesh = new MultiMesh();
            multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
            multiMesh.Mesh = CreateLeafMesh();
            multiMesh.InstanceCount = _leafTransforms.Count;

            for (int i = 0; i < _leafTransforms.Count; i++)
            {
                multiMesh.SetInstanceTransform(i, _leafTransforms[i]);
            }

            var mmInstance = new MultiMeshInstance3D();
            mmInstance.Multimesh = multiMesh;
            AddChild(mmInstance);
        }
    }

    private void BuildBranch(SurfaceTool st, Vector3 start, Vector3 dir, float length, float radius, int depth)
    {
        if (depth <= 0)
        {
            // End of branch, add leaf cluster
            // Random rotation
            var basis = Basis.Identity;
            basis = basis.Rotated(Vector3.Up, (float)_rng.NextDouble() * Mathf.Tau);
            basis = basis.Rotated(Vector3.Right, (float)_rng.NextDouble() * Mathf.Pi * 0.5f);

            _leafTransforms.Add(new Transform3D(basis, start));
            return;
        }

        Vector3 end = start + dir * length;

        // Create Cylinder Segment
        CreateCylinderSegment(st, start, end, radius, radius * 0.7f);

        // Branching
        int branchCount = _rng.Next(2, 4);
        for(int i=0; i<branchCount; i++)
        {
            // Random direction skew
            Vector3 newDir = (dir + new Vector3(
                ((float)_rng.NextDouble()-0.5f),
                ((float)_rng.NextDouble()-0.5f),
                ((float)_rng.NextDouble()-0.5f)
            )).Normalized();

            BuildBranch(st, end, newDir, length * 0.7f, radius * 0.7f, depth - 1);
        }
    }

    private void CreateCylinderSegment(SurfaceTool st, Vector3 start, Vector3 end, float r1, float r2)
    {
        int segments = 6;
        Vector3 axis = (end - start).Normalized();
        Vector3 t1 = axis.Cross(Vector3.Right).Normalized();
        if (t1.LengthSquared() < 0.01f) t1 = axis.Cross(Vector3.Up).Normalized();
        Vector3 t2 = axis.Cross(t1).Normalized();

        for (int i = 0; i < segments; i++)
        {
            float angle1 = (float)i / segments * Mathf.Tau;
            float angle2 = (float)((i + 1) % segments) / segments * Mathf.Tau;

            Vector3 offset1 = (t1 * Mathf.Cos(angle1) + t2 * Mathf.Sin(angle1));
            Vector3 offset2 = (t1 * Mathf.Cos(angle2) + t2 * Mathf.Sin(angle2));

            Vector3 v1 = start + offset1 * r1;
            Vector3 v2 = start + offset2 * r1;
            Vector3 v3 = end + offset2 * r2;
            Vector3 v4 = end + offset1 * r2;

            st.AddVertex(v1); st.AddVertex(v2); st.AddVertex(v3);
            st.AddVertex(v1); st.AddVertex(v3); st.AddVertex(v4);
        }
    }

    private Mesh CreateLeafMesh()
    {
        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);

        // Simple Quad Leaf
        st.AddVertex(new Vector3(-0.2f, 0, 0));
        st.AddVertex(new Vector3(0.2f, 0, 0));
        st.AddVertex(new Vector3(0, 0.5f, 0));

        st.GenerateNormals();
        var m = st.Commit();

        var mat = new StandardMaterial3D();
        mat.AlbedoColor = new Color(0.2f, 0.6f, 0.1f);
        mat.CullMode = BaseMaterial3D.CullModeEnum.Disabled;
        m.SurfaceSetMaterial(0, mat);

        return m;
    }
}
