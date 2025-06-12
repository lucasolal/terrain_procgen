using UnityEngine;
using MathNet.Numerics.Interpolation;

public class BasicElevation
{
    private Perlin2D stoneNoise;
    private Perlin2D mountainNoise;
    private Perlin2D dirtNoise;
    private Perlin2D temperatureNoise;
    private CubicSpline splineStone;
    private CubicSpline splineStoneSmooth;
    private CubicSpline splineDirt;

    private CubicSpline mountainSpline;


    private Perlin2D riverNoise;

    public BasicElevation(int seed, float scale)
    {
        stoneNoise = new Perlin2D(seed, World.Instance.terrainScale);
        dirtNoise = new Perlin2D(seed * 2, scale * 2);
        mountainNoise = new Perlin2D(seed + 500, World.Instance.mountainScale);
        temperatureNoise = new Perlin2D(seed + 100, World.Instance.temperatureScale);

        splineStone = CubicSpline.InterpolateNaturalSorted
        (
            new double[] {  -0.6, -0.2, -0.1, 0.0, 0.1, 0.2, 0.3, 0.6},
            new double[] {  0.2, 0.3, 0.5, 0.5, 0.55, 0.7, 0.6, 0.4}
        );
        splineStoneSmooth = CubicSpline.InterpolateNaturalSorted
        (
            new double[] {-0.6, 0.0, 0.6},
            new double[] {  0.3, 0.7, 0.3}
        );
        mountainSpline = CubicSpline.InterpolateNaturalSorted
        (
            new double[] { -1.0, 0.1, 0.3, 0.35, 0.5, 0.7 },
            new double[] { -0.1, -0.1, 0.5, 0.3, 0.1, 1.5 }
        );
        splineDirt = CubicSpline.InterpolateNaturalSorted
        (
            new double[] { -0.6, 0.0, 0.6 },
            new double[] { 5.0, 7.0, 12.0 }
        );
        stoneNoise.SampleStats();
    }
    public Voxel.VoxelType VoxelAt(int x, int y, int z)
    {
        if (World.Instance.terrainOctaves == 0) return Voxel.VoxelType.Air;
        float stoneNoiseValue = stoneNoise.OctavePerlin(x, y, World.Instance.terrainOctaves, World.Instance.terrainPersistence, World.Instance.terrainLacunarity);
        float dirtNoiseValue = dirtNoise.OctavePerlin(x, y, 1, 0.5f, 2);
        float mountainNoiseValue = 0;
        if (World.Instance.mountainOctaves != 0) mountainNoiseValue = mountainNoise.OctavePerlin(x, y, World.Instance.mountainOctaves , World.Instance.mountainPersistence, World.Instance.mountainLacunarity);
        CubicSpline stoneSplineToUse = World.Instance.smoothTerrain ? splineStoneSmooth : splineStone;
        float stoneValue = (float)stoneSplineToUse.Interpolate((double)stoneNoiseValue);
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
        int dirtHeight = (int)splineDirt.Interpolate(dirtNoiseValue);
        int stoneHeight = totalHeight - dirtHeight;
        float temperatureValue = (float)temperatureNoise.OctavePerlin(x, y, World.Instance.temperatureOctaves, World.Instance.temperaturePersistence, World.Instance.temperatureLacunarity);

        BiomePainter painter = BiomePicker.PickBiome(finalValue, mountainValue, temperatureValue);

        if (z <= stoneHeight)
        {
            return Voxel.VoxelType.Stone;
        }
        else if (z < totalHeight)
        {
            return painter.soilMaterial;
        }
        else if (z == totalHeight)
        {
            return painter.coverMaterial;
        }
        else
        {
            return Voxel.VoxelType.Air;
        }
    }


}