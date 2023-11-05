using UnityEngine;

[System.Serializable]
public struct MeshData
{
    public Vector3[] vertices;
    public Vector3[] norms;
    public int[] indices;
    public Color[] colors;
    public int arraySize;

    public bool Initialized { get; private set; }

    public void Initialize()
    {
        int maxTris = WorldSettings.ContainerSize * WorldSettings.MaxHeight * WorldSettings.ContainerSize / 4;
        arraySize = maxTris * 3;
    }

    public void ClearArrays()
    {
        indices = null;
        vertices = null;
        norms = null;
        colors = null;
    }
}
