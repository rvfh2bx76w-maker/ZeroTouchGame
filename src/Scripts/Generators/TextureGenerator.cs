using Godot;

public partial class TextureGenerator : Node
{
    public Texture2D GenerateTerrainTexture(int width, int height)
    {
        var image = Image.Create(width, height, false, Image.Format.Rgb8);
        var noise = new FastNoiseLite();
        noise.Seed = 1234;
        noise.Frequency = 0.05f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float val = noise.GetNoise2D(x, y); // -1 to 1
                Color color;

                if (val < -0.2f)
                    color = new Color(0.2f, 0.5f, 0.2f); // Grass
                else if (val < 0.4f)
                    color = new Color(0.4f, 0.35f, 0.25f); // Dirt
                else
                    color = new Color(0.5f, 0.5f, 0.5f); // Rock

                // Add noise grain
                float grain = (float)GD.RandRange(0.9f, 1.1f);
                color *= grain;

                image.SetPixel(x, y, color);
            }
        }

        return ImageTexture.CreateFromImage(image);
    }
}
