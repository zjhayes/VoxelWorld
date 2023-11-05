
public class WorldSettings
{
    public static readonly int ContainerSize = 16;
    public static readonly int MaxHeight = 256;
    public static readonly int RenderDistance = 32;

    public static int ChunkCount
    {
        get { return (ContainerSize + 3) * (MaxHeight + 1) * (ContainerSize + 3); }
    }
}
