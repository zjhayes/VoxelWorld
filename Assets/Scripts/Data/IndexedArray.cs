using UnityEngine;

[System.Serializable]
public class IndexedArray<T> where T : struct
{
    private bool initialized = false;

    [SerializeField]
    [HideInInspector]
    private T[] array;

    [SerializeField]
    [HideInInspector]
    private Vector2Int size;

    public T[] Array { get { return array; } }

    public IndexedArray()
    {
        Create(WorldManager.Instance.Settings.ChunkSize, WorldManager.Instance.Settings.MaxHeight);
    }

    public IndexedArray(int sizeX, int sizeY)
    {
        Create(sizeX, sizeY);
    }

    private void Create(int sizeX, int sizeY)
    {
        size = new Vector2Int(sizeX + 3, sizeY + 1);
        array = new T[Count];
        initialized = true;
    }

    private int IndexFromCoord(Vector3 idx)
    {
        return Mathf.RoundToInt(idx.x) + (Mathf.RoundToInt(idx.y) * size.x) + (Mathf.RoundToInt(idx.z) * size.x * size.y);
    }

    public void Clear()
    {
        if (!initialized)
            return;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.x; z++)
                {
                    array[x + (y * size.x) + (z * size.x * size.y)] = default(T);
                }
            }
        }
    }

    public int Count
    {
        get { return size.x * size.y * size.x; }
    }

    public T[] GetData
    {
        get
        {
            return array;
        }
    }

    public T this[Vector3 coord]
    {
        get
        {
            if(coord.x < 0 || coord.x > size.x ||
                coord.y < 0 || coord.y > size.y ||
                coord.z < 0 || coord.z > size.x)
            {
                Debug.LogError($"Coordinates out of bounds! {coord}");
                return default(T);
            }
            else
                return array[IndexFromCoord(coord)];
        }
        set
        {
            if(coord.x < 0 || coord.x >= size.x ||
                coord.y < 0 || coord.y >= size.y ||
                coord.z < 0 || coord.z >= size.x)
            {
                Debug.Log($"Coordinates out of bounds! {coord}");
                return;
            }
            else
                array[IndexFromCoord(coord)] = value;
        }
    }
}
