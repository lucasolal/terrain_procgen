using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public int worldSizeX = 128;
    public int worldSizeZ = 128;
    public int worldHeight = 200;
    private WorldGenerator wg;
    public static World Instance { get; private set; }
    public int seed;
    public Material VoxelMaterial;
    public float noiseScale = 0.015f;
    public float[,] noiseArray;
    CameraFlyThrough cameraController;
    Vector3 cameraPosition;
    private Vector3Int lastCameraChunkCoordinates;
    public float terrainScale;
    public float mountainScale;
    public float terrainPersistence;
    public float mountainPersistence;
    public float terrainLacunarity;
    public float mountainLacunarity;
    public int numSmallWaterBodies;
    public int numLargeWaterBodies;
    public float temperatureScale;
    public float temperaturePersistence;
    public float temperatureLacunarity;
    public int terrainOctaves;
    public int mountainOctaves;
    public int temperatureOctaves;
    public bool smoothTerrain;
    public int largeWaterMinVolume;
    public int smallWaterMinVolume;
    public int largeWaterMaxVolume;
    public int smallWaterMaxVolume;
    public bool noWater;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        LoadWorld();
    }

    void LoadWorld()
    {
        GameObject wgObject = new GameObject("WorldGenerator");
        WorldGenerator wg = wgObject.AddComponent<WorldGenerator>();
        wgObject.transform.position = new Vector3Int(0, 0, 0);
        wgObject.transform.parent = this.transform;
        wg.Initialize();
        wg.gameObject.SetActive(true);
    }

}