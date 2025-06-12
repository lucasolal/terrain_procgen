using UnityEngine;
using System;

public class Perlin2D
{
    private int seed;
    public int[] p;
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

    public static float lerp(float a, float b, float x)
    {
        return a + x * (b - a);
    }

    public static float fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    public static float gradOld(int hash, float x, float y)
    {
        int h = hash & 7;
        float u = h < 4 ? x : y;
        float v = h < 4 ? y : x;

        return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2.0f * v : 2.0f * v);
    }


    public static float grad(int hash, float x, float y)
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

    public Texture2D GenerateNoiseMap(int width, int height)
    {
        Texture2D noiseMap = new Texture2D(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                //float noiseValue = (OctavePerlin(x, y, 4, 0.5f));
                float noiseValue = (perlin(x, y));

                Color color = new Color((float)noiseValue, (float)noiseValue, (float)noiseValue);
                if (-0.001f < noiseValue &&  noiseValue < 0.001f) color = Color.red;
                noiseMap.SetPixel(x, y, color);
            }
        }

        // Aplica as mudanças na textura
        noiseMap.Apply();

        return noiseMap;
    }

    public void SampleStats()
    {
        float total = 0f;
        float min = float.MaxValue;
        float max = float.MinValue;

        System.Random rng = new System.Random();

        for (int i = 0; i < 1000; i++)
        {
            float x = (float)(rng.NextDouble() * 1000);
            float y = (float)(rng.NextDouble() * 1000);

            float value = OctavePerlin(x, y, 2, 0.5f, 2);

            total += value;
            if (value < min) min = value;
            if (value > max) max = value;
        }

        float mean = total / 10000f;

        Debug.Log($"Média do ruído: {mean}");
        Debug.Log($"Valor mínimo do ruído: {min}");
        Debug.Log($"Valor máximo do ruído: {max}");
    }

}
