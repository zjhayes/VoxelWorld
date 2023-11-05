using UnityEngine;

[System.Serializable]
public struct MeshData
{
    public Mesh mesh;
    public Vector3[] vertices;
    public int[] indices;
    public Color[] color;
    public int arraySize;

    public bool Initialized { get; private set; }

    public void Initialize()
    {
        int maxTris = WorldSettings.ContainerSize * WorldSettings.MaxHeight * WorldSettings.ContainerSize / 4;
        arraySize = maxTris * 3;
        mesh = new Mesh();

        indices = new int[arraySize];
        vertices = new Vector3[arraySize];
        color = new Color[arraySize];
    }

    public void ClearData()
    {
        //Completely clear the mesh reference to help prevent memory problems
        mesh.Clear();
        Object.Destroy(mesh);
        mesh = null;
    }
}
