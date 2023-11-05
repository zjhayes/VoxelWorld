using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

public class WorldManager : Singleton<WorldManager>
{
    [SerializeField]
    private Material worldMaterial;
    [SerializeField]
    private VoxelColor[] worldColors;
    [SerializeField]
    private VoxelTexture[] voxelTextures;
    [SerializeField]
    private Transform mainCamera;

    private Vector3 lastUpdatedPosition;
    private Vector3 previouslyCheckedPosition;

    //This will contain all modified voxels, structures, whatnot for all chunks, and will effectively be our saving mechanism
    public ConcurrentDictionary<Vector3, Dictionary<Vector3, Voxel>> modifiedVoxels = new ConcurrentDictionary<Vector3, Dictionary<Vector3, Voxel>>();
    public ConcurrentDictionary<Vector3, Chunk> activeChunks;
    public Queue<Chunk> chunkPool;
    public Queue<MeshData> meshDataPool;
    public List<MeshData> allMeshData;
    private ConcurrentQueue<Vector3> chunksNeedCreation = new ConcurrentQueue<Vector3>();
    private ConcurrentQueue<Vector3> deactiveChunks = new ConcurrentQueue<Vector3>();

    public int maxChunksToProcessPerFrame = 6;
    private int mainThreadID;
    private Thread checkActiveChunks;
    private bool killThreads = false;
    private bool performedFirstPass = false;

    public VoxelTexture[] VoxelTextures { get { return voxelTextures; } }

    // Start is called before the first frame update
    private void Start()
    {
        InitializeWorld();
    }

    private void InitializeWorld()
    {
        int renderSizePlusExcess = WorldSettings.RenderDistance + 3;
        int totalContainers = renderSizePlusExcess * renderSizePlusExcess;

        ComputeManager.Instance.Initialize(maxChunksToProcessPerFrame * 3);

        if (worldMaterial.shader.name.Contains("tex") && voxelTextures.Length > 0)
        {
            Debug.Log("Tryingg to use Textures!");
            worldMaterial.SetTexture("_TextureArray", GenerateTextureArray());
        }

        activeChunks = new ConcurrentDictionary<Vector3, Chunk>();
        chunkPool = new Queue<Chunk>();
        meshDataPool = new Queue<MeshData>();
        allMeshData = new List<MeshData>();

        mainThreadID = Thread.CurrentThread.ManagedThreadId;

        for (int i = 0; i < totalContainers; i++)
        {
            GenerateChunk(Vector3.zero, true);
        }

        for (int i = 0; i < maxChunksToProcessPerFrame * 3; i++)
        {
            GenerateMeshData(true);
        }

        checkActiveChunks = new Thread(CheckActiveChunksLoop);
        checkActiveChunks.Priority = System.Threading.ThreadPriority.BelowNormal;
        checkActiveChunks.Start();

    }

    private void Update()
    {
        if (mainCamera?.transform.position != lastUpdatedPosition)
        {
            //Update position so our CheckActiveChunksLoop thread has it
            lastUpdatedPosition = PositionToChunkCoord(mainCamera.transform.position);
        }

        Vector3 contToMake;

        while (deactiveChunks.Count > 0 && deactiveChunks.TryDequeue(out contToMake))
        {
            DeactivateContainer(contToMake);
        }
        for (int x = 0; x < maxChunksToProcessPerFrame; x++)
        {
            if (x < maxChunksToProcessPerFrame && chunksNeedCreation.Count > 0 && chunksNeedCreation.TryDequeue(out contToMake))
            {
                Chunk container = GetChunk(contToMake);
                container.ContainerPosition = contToMake;
                activeChunks.TryAdd(contToMake, container);
                ComputeManager.Instance.GenerateVoxelData(container, contToMake);
                x++;
            }

        }
    }

    private void CheckActiveChunksLoop()
    {
        Profiler.BeginThreadProfiling("Chunks", "ChunkChecker");
        int halfRenderSize = WorldSettings.RenderDistance / 2;
        int renderDistPlus1 = WorldSettings.RenderDistance + 1;
        Vector3 pos = Vector3.zero;

        Bounds chunkBounds = new Bounds();
        chunkBounds.size = new Vector3(renderDistPlus1 * WorldSettings.ContainerSize, 1, renderDistPlus1 * WorldSettings.ContainerSize);
        while (true && !killThreads)
        {
            if (previouslyCheckedPosition != lastUpdatedPosition || !performedFirstPass)
            {
                previouslyCheckedPosition = lastUpdatedPosition;

                for (int x = -halfRenderSize; x < halfRenderSize; x++)
                    for (int z = -halfRenderSize; z < halfRenderSize; z++)
                    {
                        pos.x = x * WorldSettings.ContainerSize + previouslyCheckedPosition.x;
                        pos.z = z * WorldSettings.ContainerSize + previouslyCheckedPosition.z;

                        if (!activeChunks.ContainsKey(pos))
                        {
                            chunksNeedCreation.Enqueue(pos);
                        }
                    }

                chunkBounds.center = previouslyCheckedPosition;

                foreach (var kvp in activeChunks)
                {
                    if (!chunkBounds.Contains(kvp.Key))
                        deactiveChunks.Enqueue(kvp.Key);
                }
            }

            if (!performedFirstPass)
                performedFirstPass = true;

            Thread.Sleep(300);
        }
        Profiler.EndThreadProfiling();
    }

    #region Chunk Pooling
    public Chunk GetChunk(Vector3 pos)
    {
        if (chunkPool.Count > 0)
        {
            return chunkPool.Dequeue();
        }
        else
        {
            return GenerateChunk(pos, false);
        }
    }

    private Chunk GenerateChunk(Vector3 position, bool enqueue = true)
    {
        if (Thread.CurrentThread.ManagedThreadId != mainThreadID)
        {
            chunksNeedCreation.Enqueue(position);
            return null;
        }
        Chunk chunk = new GameObject().AddComponent<Chunk>();
        chunk.transform.parent = transform;
        chunk.ContainerPosition = position;
        chunk.Initialize(worldMaterial, position);

        if (enqueue)
        {
            chunk.gameObject.SetActive(false);
            chunkPool.Enqueue(chunk);
        }

        return chunk;
    }

    public bool DeactivateContainer(Vector3 position)
    {
        if (activeChunks.ContainsKey(position))
        {
            if (activeChunks.TryRemove(position, out Chunk c))
            {
                c.ClearData();
                chunkPool.Enqueue(c);
                c.gameObject.SetActive(false);
                return true;
            }
            else
                return false;

        }

        return false;
    }
    #endregion

    #region MeshData Pooling

    public MeshData GetMeshData()
    {
        if(meshDataPool.Count > 0)
        {
            return meshDataPool.Dequeue();
        }
        else
        {
            return GenerateMeshData(false);
        }
    }

    private MeshData GenerateMeshData(bool enqueue = true)
    {
        MeshData meshData = new MeshData();
        meshData.Initialize();

        if(enqueue)
        {
            meshDataPool.Enqueue(meshData);
        }

        return meshData;
    }

    public void ClearAndRequeueMeshData(MeshData data)
    {
        data.ClearArrays();
        meshDataPool.Enqueue(data);
    }

    #endregion

    public static Vector3 PositionToChunkCoord(Vector3 pos)
    {
        pos /= WorldSettings.ContainerSize;
        pos = math.floor(pos) * WorldSettings.ContainerSize;
        pos.y = 0;
        return pos;
    }

    public VoxelColor[] WorldColors
    {
        get { return worldColors; }
    }

    private void OnApplicationQuit()
    {
        killThreads = true;
        checkActiveChunks?.Abort();

        foreach (var c in activeChunks.Keys)
        {
            if (activeChunks.TryRemove(c, out var cont))
            {
                cont.Dispose();
            }
        }

        // Try to force cleanup of editor memory.
#if UNITY_EDITOR
        EditorUtility.UnloadUnusedAssetsImmediate();
        GC.Collect();
#endif
    }

    public Texture2DArray GenerateTextureArray()
    {
        if (voxelTextures.Length > 0)
        {
            Texture2D tex = voxelTextures[0].texture;
            Texture2DArray texArrayAlbedo = new Texture2DArray(tex.width, tex.height, voxelTextures.Length, tex.format, tex.mipmapCount > 1);
            texArrayAlbedo.anisoLevel = tex.anisoLevel;
            texArrayAlbedo.filterMode = tex.filterMode;
            texArrayAlbedo.wrapMode = tex.wrapMode;

            for (int i = 0; i < voxelTextures.Length; i++)
            {
                Graphics.CopyTexture(voxelTextures[i].texture, 0, 0, texArrayAlbedo, i, 0);
            }

            return texArrayAlbedo;
        }
        Debug.Log("No Textures found while trying to generate Tex2DArray.");

        return null;
    }
}
