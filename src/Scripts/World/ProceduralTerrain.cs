using Godot;
using System;

public partial class ProceduralTerrain : Node3D
{
    [Export] public int Width = 128;
    [Export] public int Depth = 128;
    [Export] public float Scale = 1.0f;
    [Export] public float HeightMultiplier = 10.0f;
    [Export] public int Seed = 1234;

    private FastNoiseLite _noise;

    public override void _Ready()
    {
        _noise = new FastNoiseLite();
        _noise.Seed = Seed;
        _noise.Frequency = 0.02f;
        _noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;

        GenerateMesh();

        // Add a collider so the player doesn't fall through
        CreateCollision();
    }

    private void GenerateMesh()
    {
        var surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

        // Procedural Texture Material
        var texture = TextureGenerator.GenerateTerrainTexture(512, 512);

        var material = new StandardMaterial3D();
        material.AlbedoTexture = texture;
        material.Roughness = 1.0f;
        material.AlbedoColor = new Color(1, 1, 1); // Ensure white so texture shows
        surfaceTool.SetMaterial(material);

        for (int z = 0; z < Depth; z++)
        {
            for (int x = 0; x < Width; x++)
            {
                // Create two triangles for each quad
                // Vertex positions
                float y1 = _noise.GetNoise2D(x, z) * HeightMultiplier;
                float y2 = _noise.GetNoise2D(x + 1, z) * HeightMultiplier;
                float y3 = _noise.GetNoise2D(x, z + 1) * HeightMultiplier;
                float y4 = _noise.GetNoise2D(x + 1, z + 1) * HeightMultiplier;

                Vector3 v1 = new Vector3(x * Scale, y1, z * Scale);
                Vector3 v2 = new Vector3((x + 1) * Scale, y2, z * Scale);
                Vector3 v3 = new Vector3(x * Scale, y3, (z + 1) * Scale);
                Vector3 v4 = new Vector3((x + 1) * Scale, y4, (z + 1) * Scale);

                // UVs (Planar mapping)
                Vector2 uv1 = new Vector2((float)x / Width, (float)z / Depth);
                Vector2 uv2 = new Vector2((float)(x + 1) / Width, (float)z / Depth);
                Vector2 uv3 = new Vector2((float)x / Width, (float)(z + 1) / Depth);
                Vector2 uv4 = new Vector2((float)(x + 1) / Width, (float)(z + 1) / Depth);

                // Triangle 1
                surfaceTool.SetUV(uv1); surfaceTool.AddVertex(v1);
                surfaceTool.SetUV(uv2); surfaceTool.AddVertex(v2);
                surfaceTool.SetUV(uv3); surfaceTool.AddVertex(v3);

                // Triangle 2
                surfaceTool.SetUV(uv2); surfaceTool.AddVertex(v2);
                surfaceTool.SetUV(uv4); surfaceTool.AddVertex(v4);
                surfaceTool.SetUV(uv3); surfaceTool.AddVertex(v3);
            }
        }

        surfaceTool.GenerateNormals();

        var meshInstance = new MeshInstance3D();
        meshInstance.Mesh = surfaceTool.Commit();
        // Shift it so it centers somewhat
        meshInstance.Position = new Vector3(-Width * Scale / 2, 0, -Depth * Scale / 2);

        AddChild(meshInstance);
    }

    private void CreateCollision()
    {
        // For simplicity in a procedural mesh without physics server complexity,
        // we can cheat for the "Open Field" by creating a large box underneath
        // OR we can use CreateTrimeshCollision() on the MeshInstance.

        // Let's find the MeshInstance we just made
        if (GetChildCount() == 0) return;

        var meshInstance = GetChild(0) as MeshInstance3D;
        if (meshInstance != null)
        {
            meshInstance.CreateTrimeshCollision();

            // Set layer to World (Layer 1)
            if (meshInstance.GetChildCount() > 0)
            {
                var staticBody = meshInstance.GetChild(0) as StaticBody3D;
                if (staticBody != null)
                {
                    staticBody.CollisionLayer = 1;
                    staticBody.CollisionMask = 0;
                }
            }
        }
    }
}
