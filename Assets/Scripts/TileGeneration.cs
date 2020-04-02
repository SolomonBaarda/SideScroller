using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileGeneration : MonoBehaviour
{
    public int width;
    public int height;

    public string seed;
    public bool useRandomSeed;

    [Header("Wall Settings")]
    [Range(0, 100)]
    public int wallRandomFillPercentChance;
    [Range(1, 10)]
    public int wallSmoothingIterations;
    [Range(1, 8)]
    public int wallNearbyTileCutoff = 4;

    [Header("Ground Settings")]
    [Range(0, 100)]
    public int groundRandomFillPercentChance;
    [Range(1, 10)]
    public int groundSmoothingIterations;
    [Range(1, 8)]
    public int groundNearbyTileCutoff = 4;

    [Header("Tilemaps")]
    public Tilemap wall;
    public Tilemap wallDetail;
    public Tilemap background;
    public Tilemap ground;
    private List<Tilemap> allTilemaps;

    [Header("Tiles")]
    public Tile groundTile;
    public Tile wallTileMain;
    public Tile wallTileDetail;

    private int[,] mapGround;
    private int[,] mapWall;


    private int[,] maskAllowAll;

    private void SetupTilemaps()
    {
        allTilemaps = new List<Tilemap>();
        allTilemaps.Add(wall);
        allTilemaps.Add(wallDetail);
        allTilemaps.Add(background);
        allTilemaps.Add(ground);

        foreach (Tilemap t in allTilemaps)
        {
            t.size = new Vector3Int(width, height, 0);
            t.transform.position = new Vector3(transform.position.x - (width / 2), transform.position.y - (height / 2), 0);
        }
    }


    private void Start()
    {
        SetupTilemaps();

        GenerateNewMap();
    }

    private void GenerateNewMap()
    {
        DateTime before = System.DateTime.Now;

        // Create layermask 
        maskAllowAll = new int[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                maskAllowAll[x, y] = 1;
            }
        }

        GenerateWallMap();
        SetTilemap(wall, mapWall, wallTileDetail, wallTileMain);

        GenerateGroundMap();
        SetTilemap(ground, mapGround, groundTile, null);

        DateTime after = System.DateTime.Now;
        TimeSpan t = after - before;
        Debug.Log("It took " + t.Seconds + " seconds to generate the map");
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GenerateNewMap();
        }
    }


    private void GenerateGroundMap()
    {
        mapGround = new int[width, height];

        FillWithRandomRoise(ref mapGround, 2 * groundRandomFillPercentChance / 3);

        for (int i = 0; i < groundSmoothingIterations; i++)
        {
            SmoothMap(ref mapGround, groundNearbyTileCutoff, mapWall);
        }
    }


    private void GenerateWallMap()
    {
        mapWall = new int[width, height];

        FillWithRandomRoise(ref mapWall, wallRandomFillPercentChance);

        for (int i = 0; i < wallSmoothingIterations; i++)
        {
            SmoothMap(ref mapWall, wallNearbyTileCutoff, maskAllowAll);
        }
    }


    // Maybe use perlin noise?????
    /*


    private void PerlinNoise(ref int[,] map, float reduction)
    {
        int newPoint;
        //Used to reduced the position of the Perlin point
        //Create the Perlin
        for (int x = 0; x < map.GetLength(0); x++)
        {
            newPoint = Mathf.FloorToInt((Mathf.PerlinNoise(x, seed.GetHashCode()) - reduction) * map.GetLength(1));

            //Make sure the noise starts near the halfway point of the height
            newPoint += (map.GetLength(1) / 2);
            for (int y = newPoint; y >= 0; y--)
            {
                map[x, y] = 1;
            }
        }
    }


    private void SmoothPerlinNoise(ref int[,] map, int interval)
    {
        //Smooth the noise and store it in the int array
        if (interval > 1)
        {
            int newPoint, points;
            //Used to reduced the position of the Perlin point
            float reduction = 0.5f;

            //Used in the smoothing process
            Vector2Int currentPos, lastPos;
            //The corresponding points of the smoothing. One list for x and one for y
            List<int> noiseX = new List<int>();
            List<int> noiseY = new List<int>();

            //Generate the noise
            for (int x = 0; x < map.GetUpperBound(0); x += interval)
            {
                newPoint = Mathf.FloorToInt((Mathf.PerlinNoise(x, (seed.GetHashCode() * reduction))) * map.GetUpperBound(1));
                noiseY.Add(newPoint);
                noiseX.Add(x);
            }

            points = noiseY.Count;
        }

    }



    */









    private void SetTilemap(Tilemap t, int[,] map, Tile one, Tile zero)
    {
        t.ClearAllTiles();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (map[x, y] == 1)
                {
                    t.SetTile(new Vector3Int(x, y, 0), one);
                }
                else if (map[x, y] == 0)
                {
                    t.SetTile(new Vector3Int(x, y, 0), zero);
                }
            }
        }
    }



    private void SmoothMap(ref int[,] map, int nearbyTileCutoff, int[,] layerMask)
    {
        if (layerMask != null)
        {
            // Write results to new map and then apply it after
            // This avoids bias
            int[,] newMap = new int[map.GetLength(0), map.GetLength(1)];

            for (int y = 0; y < map.GetLength(1); y++)
            {
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    int neighbourWallTiles = GetSurroundingWallCount(map, x, y);

                    // Set the current tile if it is valid
                    if (layerMask[x, y] > 0 && neighbourWallTiles >= nearbyTileCutoff)
                    {
                        newMap[x, y] = 1;
                    }
                    else
                    {
                        newMap[x, y] = 0;
                    }
                }
            }
            // Apply the new map
            map = newMap;
        }
        else
        {
            Debug.LogError("Trying to smooth a map with a null layer mask.");
        }
    }

    private int GetSurroundingWallCount(int[,] map, int x, int y)
    {
        int wallCount = 0;

        // Loop through a 3x3
        for (int neighbourY = y - 1; neighbourY <= y + 1; neighbourY++)
        {
            for (int neighbourX = x - 1; neighbourX <= x + 1; neighbourX++)
            {
                if (neighbourX >= 0 && neighbourX < map.GetLength(0) && neighbourY >= 0 && neighbourY < map.GetLength(1))
                {
                    if (neighbourX != x || neighbourY != y)
                    {
                        // Add 0 or 1 for the tile
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                // Encourage the growth of walls around the edge
                else
                {
                    wallCount++;
                }

            }
        }

        return wallCount;
    }

    private void FillWithRandomRoise(ref int[,] map, int fillPercentChance)
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random r = new System.Random(seed.GetHashCode());

        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                // Set edges to always be wall
                if (x == 0 || x == map.GetLength(0) - 1 || y == 0 || y == map.GetLength(1) - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = (r.Next(0, 100) < fillPercentChance) ? 1 : 0;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (mapWall != null)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Gizmos.color = (mapWall[x, y] == 1) ? Color.black : Color.white;
                    Vector3 pos = new Vector3(-width / 2 + x + .5f, -height / 2 + y + .5f, 0);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }


}
