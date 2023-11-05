using UnityEngine;
public struct MeshBuffer
{
    public ComputeBuffer vertexBuffer;
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
        //width*height*width*faces*tris

        vertexBuffer ??= new ComputeBuffer(maxTris * 3, 12);
        colorBuffer ??= new ComputeBuffer(maxTris * 3, 16); ;
        indexBuffer ??= new ComputeBuffer(maxTris * 3, 4);

        Initialized = true;
    }

    public void Dispose()
    {
        vertexBuffer?.Dispose();
        colorBuffer?.Dispose();
        indexBuffer?.Dispose();
        countBuffer?.Dispose();

        Initialized = false;
    }
}