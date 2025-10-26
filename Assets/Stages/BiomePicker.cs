using UnityEngine;
public static class BiomePicker
{
    private static BiomePainter forestPainter;
    private static BiomePainter mountainPainter;
    private static BiomePainter highMountainPainter;
    private static BiomePainter desertPainter;
    private static BiomePainter snowPainter;
    private static BiomePainter temperatePainter;
    private static BiomePainter savannaPainter;

    static BiomePicker()
    {
        forestPainter = new BiomePainter();
        forestPainter.soilMaterial = Voxel.VoxelType.Dirt;
        forestPainter.coverMaterial = Voxel.VoxelType.Grass;

        mountainPainter = new BiomePainter();
        mountainPainter.soilMaterial = Voxel.VoxelType.Stone;
        mountainPainter.coverMaterial = Voxel.VoxelType.Stone;

        highMountainPainter = new BiomePainter();
        highMountainPainter.soilMaterial = Voxel.VoxelType.Stone;
        highMountainPainter.coverMaterial = Voxel.VoxelType.Snow;

        desertPainter = new BiomePainter();
        desertPainter.soilMaterial = Voxel.VoxelType.Dirt;
        desertPainter.coverMaterial = Voxel.VoxelType.DryGrass;

        snowPainter = new BiomePainter();
        snowPainter.soilMaterial = Voxel.VoxelType.Dirt;
        snowPainter.coverMaterial = Voxel.VoxelType.Snow;

        temperatePainter = new BiomePainter();
        temperatePainter.soilMaterial = Voxel.VoxelType.Dirt;
        temperatePainter.coverMaterial = Voxel.VoxelType.TemperateGrass;

        savannaPainter = new BiomePainter();
        savannaPainter.soilMaterial = Voxel.VoxelType.Dirt;
        savannaPainter.coverMaterial = Voxel.VoxelType.SavannaGrass;

    }
    public static BiomePainter PickBiome(double totalHeight, double mountainValue, double temperatureValue)
    {
        if (mountainValue > 0 && totalHeight <= 1.2)
        {
            return mountainPainter;
        }
        else if (mountainValue > 0)
        {
            return highMountainPainter;
        }
        else if (temperatureValue > World.Instance.highTemperatureThreshold)
        {
            return desertPainter;
        }
        else if (temperatureValue > World.Instance.middleTemperatureThreshold)
        {
            return savannaPainter;
        }
        else if (temperatureValue < -1 * World.Instance.highTemperatureThreshold)
        {
            return snowPainter;
        }
         else if (temperatureValue < -1 * World.Instance.middleTemperatureThreshold)
        {
            return temperatePainter;
        }
        else
        {
            return forestPainter;
        }
    }

}