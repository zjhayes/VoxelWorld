using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelProperties
{
    public static readonly Vector3[] Vertices = new Vector3[8]
    {
        new Vector3(0,0,0), // 0 - Bottom left
        new Vector3(1,0,0), // 1 - Bottom right
        new Vector3(0,1,0), // 2 - Top right
        new Vector3(1,1,0), // 3 - Top left
        // Front side
        new Vector3(0,0,1), // 4 - Bottom left
        new Vector3(1,0,1), // 5 - Bottom right
        new Vector3(0,1,1), // 6 - Top right
        new Vector3(1,1,1)  // 7 - Top left
    };

    public static readonly int[,] VertexIndex = new int[6, 4]
    {
        // Maps face to vertex.
        {0,1,2,3},
        {4,5,6,7},
        {4,0,6,2},
        {5,1,7,3},
        {0,1,4,5},
        {2,3,6,7}
    };

    public static readonly Vector2[] UVs = new Vector2[4]
    {
        new Vector2(0,0),
        new Vector2(0,1),
        new Vector2(1,0),
        new Vector2(1,1)
    };

    public static readonly int[,] Tris = new int[6, 6]
    {
        // Map vertex index to array of triangles.
        {0,2,3,0,3,1},
        {0,1,2,1,3,2},
        {0,2,3,0,3,1},
        {0,1,2,1,3,2},
        {0,1,2,1,3,2},
        {0,2,3,0,3,1}
    };

    public static readonly Vector3[] FaceChecks = new Vector3[6]
    {
        new Vector3(0,0,-1), // Back
        new Vector3(0,0,1), // Front
        new Vector3(-1,0,0), // Left
        new Vector3(1,0,0), // Right
        new Vector3(0,-1,0), // Bottom
        new Vector3(0,1,0) // Top
    };
}
