using UnityEngine;
public struct MeshBuffer
{
    public ComputeBuffer vertexBuffer;
    public ComputeBuffer normalBuffer;
    public ComputeBuffer colorBuffer;
    public ComputeBuffer indexBuffer;
    public ComputeBuffer countBuffer;

    public bool Initialized;
    public bool Cleared;

    public void InitializeBuffer()
    {
        if (Initialized)
            return;

        countBuffer = new ComputeBuffer(2, 4, ComputeBufferType.Counter);
        countBuffer.SetCounterValue(0);
        countBuffer.SetData(new uint[] { 0, 0 });

        int maxTris = WorldSettings.ContainerSize * WorldSettings.MaxHeight * WorldSettings.ContainerSize / 4;
        int maxVertices = WorldSettings.SharedVertices ? maxTris / 3 : maxTris;
        int maxNormals = WorldSettings.SharedVertices ? maxVertices * 3 : 1;
        //width*height*width*faces*tris

        vertexBuffer ??= new ComputeBuffer(maxVertices * 3, 12);
        colorBuffer ??= new ComputeBuffer(maxVertices * 3, 16);
        normalBuffer ??= new ComputeBuffer(maxNormals, 12);
        indexBuffer ??= new ComputeBuffer(maxTris * 3, 12);

        Initialized = true;
    }

    public void Dispose()
    {
        vertexBuffer?.Dispose();
        normalBuffer?.Dispose();
        colorBuffer?.Dispose();
        indexBuffer?.Dispose();
        countBuffer?.Dispose();

        Initialized = false;
    }
}