using UnityEngine;
using MathNet.Numerics.Interpolation;

public class BasicElevation
{
    private Perlin2D stoneNoise;
    private Perlin2D mountainNoise;
    private Perlin2D dirtNoise;
    private Perlin2D temperatureNoise;
    private CubicSpline stoneSpline;
    private CubicSpline dirtSpline;
    private CubicSpline mountainSpline;

    public BasicElevation(int seed, float scale)
    {
        stoneNoise = new Perlin2D(seed, World.Instance.terrainScale);
        dirtNoise = new Perlin2D(seed * 2, scale * 2);
        mountainNoise = new Perlin2D(seed + 500, World.Instance.mountainScale);
        temperatureNoise = new Perlin2D(seed + 100, World.Instance.temperatureScale);

        // === stoneSpline ===
        if (World.Instance.terrainSplinePoints == null || World.Instance.terrainSplinePoints.Length == 0)
            throw new System.Exception("Preencha o terrainSplinePoints.");

        stoneSpline = CreateSplineFromWorldPoints(World.Instance.terrainSplinePoints);

        // === mountainSpline ===
        if (World.Instance.mountainSplinePoints == null || World.Instance.mountainSplinePoints.Length == 0)
            throw new System.Exception("Preencha o mountainSplinePoints.");

        mountainSpline = CreateSplineFromWorldPoints(World.Instance.mountainSplinePoints);

        // === dirtSpline ===
        if (World.Instance.dirtSplinePoints == null || World.Instance.dirtSplinePoints.Length == 0)
            throw new System.Exception("Preencha o dirtSplinePoints.");

        dirtSpline = CreateSplineFromWorldPoints(World.Instance.dirtSplinePoints);

        string path = Application.dataPath + "/amostra_ruido_terreno.csv";
        stoneNoise.ExportOctavePerlinSamplesToCSV(path, 5000, World.Instance.terrainOctaves, World.Instance.terrainPersistence, World.Instance.terrainLacunarity);
    }

    private CubicSpline CreateSplineFromWorldPoints(TerrainSplinePoint[] points)
    {
        int numPoints = points.Length;
        double[] xValues = new double[numPoints];
        double[] yValues = new double[numPoints];

        for (int i = 0; i < numPoints; i++)
        {
            xValues[i] = points[i].x;
            yValues[i] = points[i].y;
        }

        return CubicSpline.InterpolateAkimaSorted(xValues, yValues);
    }
    public Voxel.VoxelType[] ColumnAt(int x, int y)
    {
        Voxel.VoxelType[] column = new Voxel.VoxelType[World.Instance.worldHeight];
        for (int z = 0; z < World.Instance.worldHeight; z++)
        {
            column[z] = Voxel.VoxelType.Air;
        }
        if (World.Instance.terrainOctaves == 0) return column;
        float stoneNoiseValue = stoneNoise.OctavePerlin(x, y, World.Instance.terrainOctaves, World.Instance.terrainPersistence, World.Instance.terrainLacunarity);
        float dirtNoiseValue = dirtNoise.OctavePerlin(x, y, 1, 0.5f, 2);
        float mountainNoiseValue = 0;
        if (World.Instance.mountainOctaves != 0) mountainNoiseValue = mountainNoise.OctavePerlin(x, y, World.Instance.mountainOctaves, World.Instance.mountainPersistence, World.Instance.mountainLacunarity);
        float stoneValue = (float)stoneSpline.Interpolate((double)stoneNoiseValue);
        float mountainValue = 0;
        if (true || World.Instance.mountainOctaves != 0)
        {
            mountainValue = Mathf.Max((float)mountainSpline.Interpolate((double)mountainNoiseValue), 0.0f);
        }
        else
        {
            mountainValue = 0;
        }

        float finalValue = stoneValue + mountainValue;
        int finalStoneHeight = (int)((finalValue) * 100);
        int totalHeight = finalStoneHeight;
        int dirtHeight = (int)dirtSpline.Interpolate(dirtNoiseValue);
        int stoneHeight = totalHeight - dirtHeight;
        float temperatureValue = (float)temperatureNoise.OctavePerlin(x, y, World.Instance.temperatureOctaves, World.Instance.temperaturePersistence, World.Instance.temperatureLacunarity);
        BiomePainter painter = BiomePicker.PickBiome(finalValue, mountainValue, temperatureValue);

        for (int z = 0; z < World.Instance.worldHeight; z++)
        {
            if (z <= stoneHeight)
            {
                column[z] = Voxel.VoxelType.Stone;
            }
            else if (z < totalHeight)
            {
                column[z] = painter.soilMaterial;
            }
            else if (z == totalHeight)
            {
                column[z] = painter.coverMaterial;
            }
            else
            {
                column[z] = Voxel.VoxelType.Air;
            }
        }
        return column;
    }


}