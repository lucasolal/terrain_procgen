using UnityEngine;
using System;
using System.IO;

public class Perlin2D
{
    private int seed;
    private int[] p;
    private float scale;

    private static readonly Vector2[] gradients = new Vector2[] {
        new Vector2( 1,  0),
        new Vector2(-1,  0),
        new Vector2( 0,  1),
        new Vector2( 0, -1),
        new Vector2( 0.7071f,  0.7071f),
        new Vector2(-0.7071f,  0.7071f),
        new Vector2( 0.7071f, -0.7071f),
        new Vector2(-0.7071f, -0.7071f)
};

    public Perlin2D(int seed, float scale)
    {
        this.seed = seed;
        this.scale = scale;
        GenerateTable();
    }

    private void GenerateTable()
    {
        p = new int[512];
        int[] perm = new int[256];


        for (int i = 0; i < 256; i++)
        {
            perm[i] = i;
        }

        System.Random rng = new System.Random(seed);
        for (int i = 255; i > 0; i--)
        {
            int swapIndex = rng.Next(i + 1);
            int temp = perm[i];
            perm[i] = perm[swapIndex];
            perm[swapIndex] = temp;
        }

        // estende o array para não haver overflow
        for (int i = 0; i < 512; i++)
        {
            p[i] = perm[i % 256];
        }
    }

    private float lerp(float a, float b, float x)
    {
        return a + x * (b - a);
    }

    private float fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private float grad(int hash, float x, float y)
    {
        Vector2 g = gradients[hash & 7];
        return g.x * x + g.y * y;
    }

    public float perlin(float x, float y)
    {
        x = x * scale; y = y * scale;
        int x0 = (int)Math.Floor(x);
        int y0 = (int)Math.Floor(y);
        int xi = x0 & 255;
        int yi = y0 & 255;
        float xf = x - x0;
        float yf = y - y0;

        float u = fade(xf);
        float v = fade(yf);

        int aa, ba, ab, bb;

        aa = p[p[xi] + yi];
        ba = p[p[xi + 1] + yi];
        ab = p[p[xi] + yi + 1];
        bb = p[p[xi + 1] + yi + 1];


        float x1, x2;
        x1 = lerp(grad(aa, xf, yf), grad(ba, xf - 1, yf), u);
        x2 = lerp(grad(ab, xf, yf - 1), grad(bb, xf - 1, yf - 1), u);

        return lerp(x1, x2, v);
    }

    public float OctavePerlin(float x, float y, int octaves, float persistence, float lacunarity)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;
        for (int i = 0; i < octaves; i++)
        {
            total += perlin(x * frequency, y * frequency) * amplitude;

            maxValue += amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return total / maxValue;
    }

    public void ExportOctavePerlinSamplesToCSVOld(string filePath, int width, int height, int octaves, float persistence, float lacunarity)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float noiseValue = OctavePerlin(x, y, octaves, persistence, lacunarity);
                    writer.WriteLine(noiseValue.ToString((System.Globalization.CultureInfo.InvariantCulture)));
                }
            }
        }

        Debug.Log($"Arquivo CSV salvo em: {filePath}");
    }

    public void ExportOctavePerlinSamplesToCSV(string filePath, int sampleCount, int octaves, float persistence, float lacunarity)
    {
        System.Random rng = new System.Random(seed + 42); // seed derivada para amostragem
        float sampleRange = 4096f;

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            for (int i = 0; i < sampleCount; i++)
            {
                float x = (float)(rng.NextDouble() * sampleRange);
                float y = (float)(rng.NextDouble() * sampleRange);
                float noiseValue = OctavePerlin(x, y, octaves, persistence, lacunarity);
                writer.WriteLine(noiseValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
        }

        Debug.Log($"Arquivo CSV salvo com {sampleCount} amostras aleatórias em: {filePath}");
    }

}
