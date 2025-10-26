using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class WorldGenerator : MonoBehaviour
{
    private Voxel[,,] voxels;
    private Color gizmoColor;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private List<Color> colors = new List<Color>();
    private Color dirtColor;
    private Color dryGrassColor;
    private Color temperateGrassColor;
    private Color savannaGrassColor;
    private BasicElevation basicElevation;

    struct Segment3D
    {
        public int z, xl, xr, y, dz;

        public Segment3D(int z, int xl, int xr, int y, int dz)
        {
            this.z = z;
            this.xl = xl;
            this.xr = xr;
            this.y = y;
            this.dz = dz;
        }
    }

    void Awake()
    {
        basicElevation = new BasicElevation(World.Instance.seed, 0.01f);
    }
    void Start()
    {
        dirtColor = new Color(0.6f, 0.4f, 0.0f, 1.0f);
        dryGrassColor = new Color(0.8f, 0.65f, 0.0f, 1.0f);
        temperateGrassColor = new Color(0.0f, 0.4f, 0.3f);
        savannaGrassColor = new Color(0.82f, 1.0f, 0.0f);
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        for (int i = 0; i < World.Instance.numSmallWaterBodies; i++)
        {
            WaterFloodFill(World.Instance.smallWaterMinVolume, World.Instance.smallWaterMaxVolume);
        }
        for (int i = 0; i < World.Instance.numLargeWaterBodies; i++)
        {
            WaterFloodFill(World.Instance.largeWaterMinVolume, World.Instance.largeWaterMaxVolume);
        }

        GenerateMesh();
    }

    private Color GetVoxelColor(Voxel.VoxelType type)
    {
        switch (type)
        {
            case Voxel.VoxelType.Stone:
                return Color.gray;
            case Voxel.VoxelType.Water:
                return Color.blue;
            case Voxel.VoxelType.Grass:
                return Color.green;
            case Voxel.VoxelType.Dirt:
                return dirtColor;
            case Voxel.VoxelType.Snow:
                return Color.white;
            case Voxel.VoxelType.DryGrass:
                return dryGrassColor;
            case Voxel.VoxelType.TemperateGrass:
                return temperateGrassColor;
            case Voxel.VoxelType.SavannaGrass:
                return savannaGrassColor;
            default:
                return Color.white;
        }
    }

    Voxel GetVoxel(int x, int y, int z)
    {
        return voxels[x, y, z];
    }

    private void InitializeVoxels()
    {
        for (int x = 0; x < World.Instance.worldSizeX; x++)
        {
            for (int z = 0; z < World.Instance.worldSizeZ; z++)
            {
                Voxel.VoxelType[] column = basicElevation.ColumnAt(x, z);
                for (int y = 0; y < World.Instance.worldHeight; y++)
                {
                    Vector3 worldPos = transform.position + new Vector3(x, y, z);
                    //Voxel.VoxelType type = DetermineVoxelType(worldPos.x, worldPos.y, worldPos.z);
                    //voxels[x, y, z] = new Voxel(worldPos, type, type != Voxel.VoxelType.Air);
                    voxels[x, y, z] = new Voxel(worldPos, column[y], column[y] != Voxel.VoxelType.Air);
                }
            }
        }
        gizmoColor = new Color(Random.value, Random.value, Random.value, 0.4f);
    }

    public void Initialize()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        long memoryBefore = System.GC.GetTotalMemory(true);
        voxels = new Voxel[World.Instance.worldSizeX, World.Instance.worldHeight, World.Instance.worldSizeZ];
        InitializeVoxels();
        stopwatch.Stop();
        long memoryAfter = System.GC.GetTotalMemory(true);
        double memoryUsedMB = (memoryAfter - memoryBefore) / (1024.0 * 1024.0);
        UnityEngine.Debug.Log($"WorldGenerator - Initialize - mundo {World.Instance.worldSizeX} x {World.Instance.worldSizeZ} terminou em {stopwatch.Elapsed.TotalSeconds} s. Usou  {memoryUsedMB:F2} MB");
    }

    public void IterateVoxels()
    {
        for (int x = 0; x < World.Instance.worldSizeX; x++)
        {
            for (int y = 0; y < World.Instance.worldHeight; y++)
            {
                for (int z = 0; z < World.Instance.worldSizeZ; z++)
                {
                    ProcessVoxel(x, y, z);
                }
            }
        }
    }

    private void ProcessVoxel(int x, int y, int z)
    {
        if (voxels == null || x < 0 || x >= voxels.GetLength(0) ||
            y < 0 || y >= voxels.GetLength(1) || z < 0 || z >= voxels.GetLength(2))
        {
            return;
        }
        Voxel voxel = voxels[x, y, z];
        if (voxel.isActive)
        {
            bool[] facesVisible = new bool[6];

            facesVisible[0] = IsFaceVisible(x, y + 1, z); // cima
            facesVisible[1] = IsFaceVisible(x, y - 1, z); // baixo
            facesVisible[2] = IsFaceVisible(x - 1, y, z); // esq
            facesVisible[3] = IsFaceVisible(x + 1, y, z); // dir
            facesVisible[4] = IsFaceVisible(x, y, z + 1); // frente
            facesVisible[5] = IsFaceVisible(x, y, z - 1); // tras

            for (int i = 0; i < facesVisible.Length; i++)
            {
                if (facesVisible[i])
                    AddFaceData(x, y, z, i);
            }
        }
    }

    private bool IsFaceVisible(int x, int y, int z)
    {
        Vector3 globalPos = transform.position + new Vector3(x, y, z);

        return IsVoxelHiddenInChunk(x, y, z);
    }

    private bool IsVoxelHiddenInChunk(int x, int y, int z)
    {
        if (x < 0 || x >= World.Instance.worldSizeX || y < 0 || y >= World.Instance.worldHeight || z < 0 || z >= World.Instance.worldSizeZ)
            return true;
        return !voxels[x, y, z].isActive;
    }

    public bool IsVoxelActiveAt(Vector3 localPosition)
    {
        int x = Mathf.RoundToInt(localPosition.x);
        int y = Mathf.RoundToInt(localPosition.y);
        int z = Mathf.RoundToInt(localPosition.z);

        return voxels[x, y, z].isActive;
    }

    private void AddFaceData(int x, int y, int z, int faceIndex)
    {

        Voxel voxel = voxels[x, y, z];
        Color voxelColor = GetVoxelColor(voxel.type);

        if (faceIndex == 0)
        {
            vertices.Add(new Vector3(x, y + 1, z));
            vertices.Add(new Vector3(x, y + 1, z + 1));
            vertices.Add(new Vector3(x + 1, y + 1, z + 1));
            vertices.Add(new Vector3(x + 1, y + 1, z));
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 1));
        }

        if (faceIndex == 1)
        {
            vertices.Add(new Vector3(x, y, z));
            vertices.Add(new Vector3(x + 1, y, z));
            vertices.Add(new Vector3(x + 1, y, z + 1));
            vertices.Add(new Vector3(x, y, z + 1));
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(1, 0));
        }

        if (faceIndex == 2)
        {
            vertices.Add(new Vector3(x, y, z));
            vertices.Add(new Vector3(x, y, z + 1));
            vertices.Add(new Vector3(x, y + 1, z + 1));
            vertices.Add(new Vector3(x, y + 1, z));
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(0, 1));
        }

        if (faceIndex == 3)
        {
            vertices.Add(new Vector3(x + 1, y, z + 1));
            vertices.Add(new Vector3(x + 1, y, z));
            vertices.Add(new Vector3(x + 1, y + 1, z));
            vertices.Add(new Vector3(x + 1, y + 1, z + 1));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(1, 0));
        }

        if (faceIndex == 4)
        {
            vertices.Add(new Vector3(x, y, z + 1));
            vertices.Add(new Vector3(x + 1, y, z + 1));
            vertices.Add(new Vector3(x + 1, y + 1, z + 1));
            vertices.Add(new Vector3(x, y + 1, z + 1));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(1, 1));
        }

        if (faceIndex == 5)
        {
            vertices.Add(new Vector3(x + 1, y, z));
            vertices.Add(new Vector3(x, y, z));
            vertices.Add(new Vector3(x, y + 1, z));
            vertices.Add(new Vector3(x + 1, y + 1, z));
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(0, 0));

        }

        for (int i = 0; i < 4; i++) colors.Add(voxelColor);

        AddTriangleIndices();
    }

    private void AddTriangleIndices()
    {
        int vertCount = vertices.Count;

        triangles.Add(vertCount - 4);
        triangles.Add(vertCount - 3);
        triangles.Add(vertCount - 2);

        triangles.Add(vertCount - 4);
        triangles.Add(vertCount - 2);
        triangles.Add(vertCount - 1);
    }

    private void GenerateMesh()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        IterateVoxels();

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        mesh.colors = colors.ToArray();
        meshRenderer.material = World.Instance.VoxelMaterial;
        stopwatch.Stop();
        UnityEngine.Debug.Log($"WorldGenerator - GenerateMesh - mundo {World.Instance.worldSizeX} x {World.Instance.worldSizeZ} terminou em {stopwatch.Elapsed.TotalSeconds} s.");
    }
    public void WaterFloodFill(int minVolume, int maxVolume)
    {
        if (World.Instance.noWater) return;
        System.Random rng = new System.Random(World.Instance.seed + 2);
        int attempts = 1000;
        long startMemory = System.GC.GetTotalMemory(false);
        HashSet<(Vector3Int, Vector3Int)> visitedEdges = new HashSet<(Vector3Int, Vector3Int)>();

        for (int attempt = 0; attempt < attempts; attempt++)
        {
            long peakMemory = System.GC.GetTotalMemory(false);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int startX = rng.Next(0, World.Instance.worldSizeX);
            int startZ = rng.Next(0, World.Instance.worldSizeZ);

            int surfaceY = -1;
            if (World.Instance.terrainOctaves == 0)
            {
                //para testes. Quando tem 0 oitavas o mundo é vazio e quero preenche-lo todo com água
                surfaceY = World.Instance.worldHeight - 1;
                minVolume = 0;
                maxVolume = 99999999;
            }
            else
            {
                for (int y = World.Instance.worldHeight - 1; y > 0; y--)
                {

                    if (IsInBounds(startX, y, startZ) &&
                        voxels[startX, y, startZ].type != Voxel.VoxelType.Air &&
                        IsInBounds(startX, y + 1, startZ) &&
                        voxels[startX, y + 1, startZ].type == Voxel.VoxelType.Air)
                    {
                        surfaceY = y + 1;
                        break;
                    }
                }
            }

            if (surfaceY == -1 || voxels[startX, surfaceY - 1, startZ].type == Voxel.VoxelType.Water) continue;

            Queue<Vector3Int> queue = new Queue<Vector3Int>();
            HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
            Vector3Int start = new Vector3Int(startX, surfaceY, startZ);
            queue.Enqueue(start);
            visited.Add(start);
            int volume = 0;

            while (queue.Count > 0)
            {
                Vector3Int current = queue.Dequeue();
                int x = current.x, y = current.y, z = current.z;
                if (y > surfaceY) continue;
                if (IsInBounds(x, y, z) && voxels[x, y, z].type == Voxel.VoxelType.Air)
                {
                    long currentMemory = System.GC.GetTotalMemory(false);
                    if (currentMemory > peakMemory) peakMemory = currentMemory;
                    volume++;
                    if (volume > maxVolume) break;
                }

                Vector3Int[] directions = {
                new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
                new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
                new Vector3Int(0, 0, 1), new Vector3Int(0, 0, -1)
            };

                foreach (var dir in directions)
                {
                    Vector3Int neighbor = current + dir;
                    var edge = (MinVector(current, neighbor), MaxVector(current, neighbor));
                    if (IsInBounds(neighbor.x, neighbor.y, neighbor.z) && neighbor.y <= surfaceY) visitedEdges.Add(edge);
                    if (IsInBounds(neighbor.x, neighbor.y, neighbor.z) &&
                        !visited.Contains(neighbor) &&
                        voxels[neighbor.x, neighbor.y, neighbor.z].type == Voxel.VoxelType.Air)
                    {
                        if (neighbor.y <= surfaceY)
                        {
                            queue.Enqueue(neighbor);
                            visited.Add(neighbor);
                        }
                    }
                }
            }

            if (volume <= maxVolume && volume >= minVolume)
            {
                foreach (var pos in visited)
                {
                    voxels[pos.x, pos.y, pos.z] = new Voxel(new Vector3(pos.x, pos.y, pos.z), Voxel.VoxelType.Water, true);
                }
                stopwatch.Stop();
                long peakMemoryMB = (peakMemory - startMemory) / (1024 * 1024);
                UnityEngine.Debug.Log($"WaterFloodFill - volume {volume} terminou em {stopwatch.Elapsed.TotalSeconds} s. Uso de memória {peakMemoryMB} MB. Qtd arestas: {visitedEdges.Count}");
                return;
            }
            else
            {
                stopwatch.Stop();
            }
        }
    }

    Vector3Int MinVector(Vector3Int a, Vector3Int b)
    {
        if (a.x != b.x) return a.x < b.x ? a : b;
        if (a.y != b.y) return a.y < b.y ? a : b;
        return a.z < b.z ? a : b;
    }

    Vector3Int MaxVector(Vector3Int a, Vector3Int b)
    {
        if (a.x != b.x) return a.x > b.x ? a : b;
        if (a.y != b.y) return a.y > b.y ? a : b;
        return a.z > b.z ? a : b;
    }


    private bool IsInBounds(int x, int y, int z)
    {
        return x >= 0 && x < World.Instance.worldSizeX &&
               y >= 0 && y < World.Instance.worldHeight &&
               z >= 0 && z < World.Instance.worldSizeZ;
    }



}



