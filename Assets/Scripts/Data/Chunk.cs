using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public Vector3 ContainerPosition { get; set; }
    public Mesh mesh;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    public void Initialize(Material mat, Vector3 position)
    {
        ConfigureComponents();
        meshRenderer.sharedMaterial = mat;
        ContainerPosition = position;
    }

    public void ClearData()
    {
        meshFilter.sharedMesh = null;
        meshCollider.sharedMesh = null;
        mesh.Clear();
        Destroy(mesh);
        mesh = null;
    }

    public void UploadMesh(MeshBuffer meshBuffer)
    {

        if (meshRenderer == null)
            ConfigureComponents();

        //Get the count of vertices/tris from the shader
        int[] faceCount = new int[2] { 0, 0 };
        meshBuffer.countBuffer.GetData(faceCount);

        MeshData meshData = WorldManager.Instance.GetMeshData();
        meshData.vertices = new Vector3[faceCount[0]];
        meshData.colors = new Color[faceCount[0]];
        meshData.norms = new Vector3[faceCount[0]];
        meshData.indices = new int[faceCount[1]];

        //Get all of the meshData from the buffers to local arrays
        meshBuffer.vertexBuffer.GetData(meshData.vertices, 0, 0, faceCount[0]);
        meshBuffer.indexBuffer.GetData(meshData.indices, 0, 0, faceCount[1]);
        meshBuffer.colorBuffer.GetData(meshData.colors, 0, 0, faceCount[0]);

        if (WorldManager.Instance.Settings.SharedVertices)
            meshBuffer.normalBuffer.GetData(meshData.norms, 0, 0, faceCount[0]);

        //Assign the mesh
        mesh = new Mesh();

        if (WorldManager.Instance.Settings.SharedVertices)
            mesh.SetNormals(meshData.norms, 0, faceCount[0]);

        mesh.SetVertices(meshData.vertices, 0, faceCount[0]);
        mesh.SetIndices(meshData.indices, 0, faceCount[1], MeshTopology.Triangles, 0);
        mesh.SetColors(meshData.colors, 0, faceCount[0]);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        mesh.UploadMeshData(true);

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;

        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        WorldManager.Instance.ClearAndRequeueMeshData(meshData);

    }

    public void Dispose()
    {
        // meshData.ClearArrays() ?
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
