using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Container : MonoBehaviour
{
    public Vector3 ContainerPosition { get; set; }
    public NoiseBuffer Data { get; private set; }

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    private MeshData meshData;

    public void Initialize(Material mat, Vector3 position)
    {
        ConfigureComponents();
        meshData = new MeshData();
        meshData.Initialize();
        meshRenderer.sharedMaterial = mat;
        ContainerPosition = position;
    }

    public void ClearData()
    {
        meshFilter.sharedMesh = null;
        meshCollider.sharedMesh = null;
        meshData.ClearData();
    }

    /*public void GenerateMesh()
    {
        Vector3 blockPos;
        Voxel block;

        int counter = 0;
        Vector3[] faceVertices = new Vector3[4];
        Vector2[] faceUVs = new Vector2[4];

        VoxelColor voxelColor;
        Color voxelColorAlpha;
        Vector2 voxelSmoothness;

        for (int x = 1; x < WorldSettings.ContainerSize + 1; x++)
        {
            for(int y = 0; y < WorldSettings.MaxHeight; y++)
            {
                for(int z = 1; z < WorldSettings.ContainerSize + 1; z++)
                {
                    blockPos = new Vector3(x, y, z);
                    block = this[blockPos];
                    // Only check on solid blocks.
                    if (!block.IsSolid)
                        continue;

                    voxelColor = WorldManager.Instance.WorldColors[block.ID - 1];
                    voxelColorAlpha = voxelColor.color;
                    voxelColorAlpha.a = 1;
                    voxelSmoothness = new Vector2(voxelColor.metallic, voxelColor.smoothness);

                    // Iterate over each face direction.
                    for (int i = 0; i < 6; i++)
                    {
                        // Check if there's a solid block against this face.
                        if (CheckVoxelIsSolid(blockPos + VoxelProperties.FaceChecks[i]))
                            continue;
                        // else draw this face.

                        // Collect the appropriate vertices from the default vertices and add the block position.
                        for (int j = 0; j < 4; j++)
                        {
                            faceVertices[j] = VoxelProperties.Vertices[VoxelProperties.VertexIndex[i, j]] + blockPos;
                            faceUVs[j] = VoxelProperties.UVs[j];
                        }

                        for (int j = 0; j < 6; j++)
                        {
                            meshData.Vertices.Add(faceVertices[VoxelProperties.Tris[i, j]]);
                            meshData.UVs.Add(faceUVs[VoxelProperties.Tris[i, j]]);
                            meshData.UVs2.Add(voxelSmoothness);
                            meshData.Colors.Add(voxelColorAlpha);

                            meshData.Triangles.Add(counter++);
                        }
                    }
                }
            }

        }
    }*/

    public void UploadMesh(MeshBuffer meshBuffer)
    {

        if (meshRenderer == null)
            ConfigureComponents();

        //Get the count of vertices/tris from the shader
        int[] faceCount = new int[2] { 0, 0 };
        meshBuffer.countBuffer.GetData(faceCount);

        //Get all of the meshData from the buffers to local arrays
        meshBuffer.vertexBuffer.GetData(meshData.vertices, 0, 0, faceCount[0]);
        meshBuffer.indexBuffer.GetData(meshData.indices, 0, 0, faceCount[0]);
        meshBuffer.colorBuffer.GetData(meshData.color, 0, 0, faceCount[0]);

        //Assign the mesh
        meshData.mesh = new Mesh();
        meshData.mesh.SetVertices(meshData.vertices, 0, faceCount[0]);
        meshData.mesh.SetIndices(meshData.indices, 0, faceCount[0], MeshTopology.Triangles, 0);
        meshData.mesh.SetColors(meshData.color, 0, faceCount[0]);

        meshData.mesh.RecalculateNormals();
        meshData.mesh.RecalculateBounds();
        meshData.mesh.Optimize();
        meshData.mesh.UploadMeshData(true);

        meshFilter.sharedMesh = meshData.mesh;
        meshCollider.sharedMesh = meshData.mesh;

        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

    }

    /*public bool CheckVoxelIsSolid(Vector3 point)
    {
        if (point.y < 0 || (point.x > WorldSettings.ContainerSize + 2) || (point.z > WorldSettings.ContainerSize + 2))
            return true;
        else
            return this[point].IsSolid;
    }*/

    public void Dispose()
    {
        meshData.ClearData();
        meshData.indices = null;
        meshData.vertices = null;
        meshData.color = null;
    }

    public Voxel this[Vector3 index]
    {
        get
        {
            if (WorldManager.Instance.modifiedVoxels.ContainsKey(ContainerPosition))
            {
                if (WorldManager.Instance.modifiedVoxels[ContainerPosition].ContainsKey(index))
                {
                    return WorldManager.Instance.modifiedVoxels[ContainerPosition][index];
                }
                else return new Voxel() { ID = 0 };
            }
            else return new Voxel() { ID = 0 };
        }

        set
        {
            if (!WorldManager.Instance.modifiedVoxels.ContainsKey(ContainerPosition))
                WorldManager.Instance.modifiedVoxels.TryAdd(ContainerPosition, new Dictionary<Vector3, Voxel>());
            if (!WorldManager.Instance.modifiedVoxels[ContainerPosition].ContainsKey(index))
                WorldManager.Instance.modifiedVoxels[ContainerPosition].Add(index, value);
            else
                WorldManager.Instance.modifiedVoxels[ContainerPosition][index] = value;
        }
    }

    private void ConfigureComponents()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public static readonly string Name = "Container";
}
