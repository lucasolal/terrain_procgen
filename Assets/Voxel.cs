using UnityEngine;
public struct Voxel
{
  public Vector3 position;
  public VoxelType type;
  public enum VoxelType
  {
    Air,
    Grass,
    Stone,
    Dirt,
    Water,
    Snow,
    Sand,
    DryGrass,
    TemperateGrass,
    SavannaGrass
  }
  public bool isActive;
  public Voxel(Vector3 position, VoxelType type, bool isActive = true)
  {
    this.position = position;
    this.type = type;
    this.isActive = isActive;
    if (type == Voxel.VoxelType.Air) {
      this.isActive = false;
    }
  }
}