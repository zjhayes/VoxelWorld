[System.Serializable]
public class WorldSettings
{
    public int ChunkSize = 16;
    public int MaxHeight = 256;
    public int RenderDistance = 32;
    public bool SharedVertices = false;
    public bool UseTextures = true;

    public int ChunkCount
    {
        get { return (ChunkSize + 3) * (MaxHeight + 1) * (ChunkSize + 3); }
    }
}
