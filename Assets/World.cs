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
    public int largeWaterMinVolume;
    public int smallWaterMinVolume;
    public int largeWaterMaxVolume;
    public int smallWaterMaxVolume;
    public bool noWater;
    public float middleTemperatureThreshold;
    public float highTemperatureThreshold;
    public TerrainSplinePoint[] terrainSplinePoints;
    public TerrainSplinePoint[] mountainSplinePoints;
    public TerrainSplinePoint[] dirtSplinePoints;
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

        //Inicialização dos parâmetros default se não forem preenchidos
        worldSizeX = (worldSizeX <= 0) ? 128 : worldSizeX;
        worldSizeZ = (worldSizeZ <= 0) ? 128 : worldSizeZ;
        worldHeight = (worldHeight <= 0) ? 200 : worldHeight;
        seed = (seed == 0) ? 12345 : seed;

        noiseScale = (noiseScale <= 0) ? 0.015f : noiseScale;
        terrainScale = (terrainScale <= 0) ? 0.005f : terrainScale;
        mountainScale = (mountainScale <= 0) ? 0.004f : mountainScale;
        temperatureScale = (temperatureScale <= 0) ? 0.005f : temperatureScale;

        terrainPersistence = (terrainPersistence <= 0) ? 0.5f : terrainPersistence;
        mountainPersistence = (mountainPersistence <= 0) ? 0.5f : mountainPersistence;
        temperaturePersistence = (temperaturePersistence <= 0) ? 0.5f : temperaturePersistence;

        terrainLacunarity = (terrainLacunarity <= 0) ? 2.0f : terrainLacunarity;
        mountainLacunarity = (mountainLacunarity <= 0) ? 2.0f : mountainLacunarity;
        temperatureLacunarity = (temperatureLacunarity <= 0) ? 2.0f : temperatureLacunarity;

        //terrainOctaves = (terrainOctaves <= 0) ? 4 : terrainOctaves;
        //mountainOctaves = (mountainOctaves <= 0) ? 4 : mountainOctaves;
        temperatureOctaves = (temperatureOctaves <= 0) ? 4 : temperatureOctaves;

        //numSmallWaterBodies = (numSmallWaterBodies <= 0) ? 10 : numSmallWaterBodies;
        //numLargeWaterBodies = (numLargeWaterBodies <= 0) ? 2 : numLargeWaterBodies;

        largeWaterMinVolume = (largeWaterMinVolume <= 0) ? 50000 : largeWaterMinVolume;
        smallWaterMinVolume = (smallWaterMinVolume <= 0) ? 5000 : smallWaterMinVolume;
        largeWaterMaxVolume = (largeWaterMaxVolume <= 0) ? 200000 : largeWaterMaxVolume;
        smallWaterMaxVolume = (smallWaterMaxVolume <= 0) ? 20000 : smallWaterMaxVolume;

        middleTemperatureThreshold = (middleTemperatureThreshold == 0f) ? 0.5f : middleTemperatureThreshold;
        highTemperatureThreshold = (highTemperatureThreshold == 0f) ? 0.8f : highTemperatureThreshold;

        if (terrainSplinePoints == null || terrainSplinePoints.Length == 0)
        {
            terrainSplinePoints = new TerrainSplinePoint[]
            {
            new TerrainSplinePoint { x = -0.6f, y = 0.2f },
            new TerrainSplinePoint { x = -0.2f, y = 0.35f },
            new TerrainSplinePoint { x = -0.1f, y = 0.4f },
            new TerrainSplinePoint { x =  0.0f, y = 0.5f },
            new TerrainSplinePoint { x =  0.1f, y = 0.6f },
            new TerrainSplinePoint { x =  0.2f, y = 0.5f },
            new TerrainSplinePoint { x =  0.3f, y = 0.55f },
            new TerrainSplinePoint { x =  0.6f, y = 0.4f }
            };
        }

        if (mountainSplinePoints == null || mountainSplinePoints.Length == 0)
        {
            mountainSplinePoints = new TerrainSplinePoint[]
            {
            new TerrainSplinePoint { x = -1.0f, y = -0.1f },
            new TerrainSplinePoint { x =  0.1f, y = -0.1f },
            new TerrainSplinePoint { x =  0.3f, y = 0.5f },
            new TerrainSplinePoint { x =  0.35f, y = 0.3f },
            new TerrainSplinePoint { x =  0.5f, y = 0.1f },
            new TerrainSplinePoint { x =  0.7f, y = 1.5f }
            };
        }

        if (dirtSplinePoints == null || dirtSplinePoints.Length == 0)
        {
            dirtSplinePoints = new TerrainSplinePoint[]
            {
            new TerrainSplinePoint { x = -0.6f, y = 5.0f },
            new TerrainSplinePoint { x =  0.0f, y = 7.0f },
            new TerrainSplinePoint { x =  0.6f, y = 12.0f }
            };
        }
    }
    void Start()
    {
        LoadWorld();
    }

    void LoadWorld()
    {
        if (middleTemperatureThreshold >= highTemperatureThreshold)
        {
            throw new System.Exception("middleTemperatureThreshold não pode ser maior ou igual a highTemperatureThreshold");
        }
        GameObject wgObject = new GameObject("WorldGenerator");
        WorldGenerator wg = wgObject.AddComponent<WorldGenerator>();
        wgObject.transform.position = new Vector3Int(0, 0, 0);
        wgObject.transform.parent = this.transform;
        wg.Initialize();
        wg.gameObject.SetActive(true);
    }

}