using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class TerrainManager : MonoBehaviour
{

    /// <summary>
    /// Called when a terrain chunk has been generated.  
    /// </summary>
    public static UnityAction<TerrainChunk> OnTerrainChunkGenerated;

    private System.Random Random;

    public static Presets.VariableValue<int> WorldLengthPreset => new Presets.VariableValue<int>(8, 4, 12, 1, 16);
    public int WorldLength { get; private set; }

    public Generation GenerationRule { get; private set; }

    private Dictionary<Vector2Int, bool> generatedChunks = new Dictionary<Vector2Int, bool>();

    public bool IsGenerating { get; private set; } = false;

    public Vector2 CellSize { get { return grid.cellSize; } }

    public static readonly Vector2Int initialTilePos = Vector2Int.zero;

    // Tilemaps
    private Grid grid;
    private Tilemap wall;
    private Tilemap wallDetail;
    private Tilemap background;
    private Tilemap hazard;
    private Tilemap ground;

    [Header("Sample Terrain Manager Reference")]
    public GameObject sampleTerrainManagerObjectPrefab;
    private SampleTerrainManager sampleTerrainManager;

    [Header("Initial Tile Type")]
    public Tile groundTile;
    public RandomTile backgroundWallTile;

    public const string LAYER_NAME_GROUND = "Ground";


    private void Awake()
    {
        // Get the references
        grid = GetComponent<Grid>();

        GameObject sampleGameObject = Instantiate(sampleTerrainManagerObjectPrefab);
        sampleTerrainManager = sampleGameObject.GetComponent<SampleTerrainManager>();

        // Assign the tilemaps 
        for (int i = 0; i < grid.transform.childCount; i++)
        {
            GameObject g = grid.transform.GetChild(i).gameObject;
            Tilemap t = g.GetComponent<Tilemap>();

            // Walls
            if (g.name.Contains("Wall"))
            {
                // Wall detail
                if (g.name.Contains("Detail"))
                {
                    wallDetail = t;
                }
                // Wall
                else
                {
                    wall = t;
                }
            }
            // Background
            else if (g.name.Contains("Background"))
            {
                background = t;
            }
            // Hazard
            else if (g.name.Contains("Hazard"))
            {
                hazard = t;
            }
            // Ground
            else if (g.name.Contains("Ground"))
            {
                ground = t;
            }
        }
    }



    public void GenerateSpawn(Generation worldgenerationType, int worldLength, int seedHash)
    {
        Random = new System.Random(seedHash);
        GenerationRule = worldgenerationType;

        if (GenerationRule.ToString().Contains("Limit"))
        {
            WorldLength = worldLength;
        }

        // Reset the tilemaps 
        ClearAllTiles();

        // Generate a single tile at the orign
        GenerateInitialTile(initialTilePos);

        // Generate the spawn area
        Generate(initialTilePos, Direction.Both, ChunkManager.initialChunkID, sampleTerrainManager.startingArea);
    }


    public void LoadSampleTerrain(bool printDebug)
    {
        if(!sampleTerrainManager.TerrainIsLoaded)
        {
            // Load the sample terrain
            DateTime before = DateTime.Now;

            sampleTerrainManager.LoadAllSampleTerrain();

            TimeSpan time = DateTime.Now - before;
            if (printDebug)
            {
                Debug.Log("It took " + time.Milliseconds + " ms to load the sample terrain.");
            }
        }
    }



    public void GenerateRandom(Vector2 startTileWorldSpace, Direction directionToGenerate, Vector2Int chunkID)
    {
        // Get a list of only the valid sample terrain
        List<SampleTerrain> allValidSamples = new List<SampleTerrain>();

        // Get all possible terrain
        foreach (SampleTerrain t in sampleTerrainManager.allSamples)
        {
            if (IsValidDirection(directionToGenerate, t.direction))
            {
                allValidSamples.Add(t);
            }
        }

        // Check if there is a limit
        if (GenerationRule.ToString().Contains("Limit"))
        {
            // Get the current world length in chunks
            int currentWorldLength = Mathf.Abs(chunkID.x - ChunkManager.initialChunkID.x) + Mathf.Abs(chunkID.y - ChunkManager.initialChunkID.y);

            // This is the final chunk to generate
            if (currentWorldLength == WorldLength)
            {
                // Ensure it is only finishing areas that get generated
                allValidSamples = new List<SampleTerrain>
                {
                    sampleTerrainManager.finishArea
                };
            }
            // We have generated enough chunks
            else if (currentWorldLength > WorldLength)
            {
                return;
            }
        }

        if (allValidSamples.Count.Equals(0))
        {
            throw new Exception("No valid Sample Terrain for generation direction " + directionToGenerate);
        }


        GenerateRandom(startTileWorldSpace, directionToGenerate, chunkID, allValidSamples);
    }


    public void GenerateRandom(Vector2 startTileWorldSpace, Direction directionToGenerate, Vector2Int chunkID, List<SampleTerrain> validTerrain)
    {
        // Randomly choose one to generate
        int index = Random.Next(0, validTerrain.Count);
        SampleTerrain chosen = validTerrain[index];

        Generate(startTileWorldSpace, directionToGenerate, chunkID, chosen);
    }


    public void Generate(Vector2 startTileWorldSpace, Direction directionToGenerate, Vector2Int chunkID, SampleTerrain terrain)
    {
        // Ensure the chunk is not already being generated 
        if (!ChunkAlreadyGenerating(chunkID))
        {
            SetChunkGenerating(chunkID);

            // Decide if we need to flip the terrain on the x axis
            bool flipAxisX = false;
            if ((directionToGenerate.Equals(Direction.Left) && terrain.direction.Equals(Direction.Right))
                || (directionToGenerate.Equals(Direction.Right) && terrain.direction.Equals(Direction.Left)))
            {
                flipAxisX = true;
            }

            Vector3Int entryPos = grid.WorldToCell(startTileWorldSpace);

            // Generate the new chunk and update the tile reference
            GenerateFromSampleTerrain(new Vector2Int(entryPos.x, entryPos.y), flipAxisX, directionToGenerate, terrain, chunkID);
        }
    }


    public bool ChunkAlreadyGenerating(Vector2Int chunkID)
    {
        return generatedChunks.TryGetValue(chunkID, out _);
    }

    public void SetChunkGenerating(Vector2Int chunkID)
    {
        if (generatedChunks.ContainsKey(chunkID))
        {
            generatedChunks.Remove(chunkID);
        }

        generatedChunks.Add(chunkID, true);
    }


    private void GenerateFromSampleTerrain(Vector2Int entryTile, bool flipAxisX, Direction directionToGenerate, SampleTerrain terrain, Vector2Int chunkID)
    {
        IsGenerating = true;

        // Copy the terrain, each layer at a time
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.wall, wall);
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.wallDetail, wallDetail);
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.background, background);
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.hazard, hazard);
        CopySampleTerrainLayer(entryTile, flipAxisX, terrain.ground, ground);

        IsGenerating = false;

        // Generate the chunk
        TerrainChunk c = GenerateTerrainChunk(entryTile, flipAxisX, directionToGenerate, terrain, chunkID);

        // Tell the ChunkManager that the terrain has been generated
        OnTerrainChunkGenerated.Invoke(c);
    }


    /// <summary>
    /// My very cool coroutine method i wrote that is now obsolete.
    /// </summary>
    /// <param name="entryPosition"></param>
    /// <param name="flipAxisX"></param>
    /// <param name="layer"></param>
    /// <param name="tilemap"></param>
    /// <param name="loadInBackground"></param>
    /// <returns></returns>
    private IEnumerator CopySampleTerrainLayerOld(Vector2Int entryPosition, bool flipAxisX, SampleTerrain.Layer layer, Tilemap tilemap, bool loadInBackground)
    {
        Vector2Int entry = new Vector2Int(entryPosition.x, entryPosition.y);

        int invert = 1;
        if (flipAxisX)
        {
            invert = -1;
        }

        float currentFrameTime = 0;
        float tilesSinceLastPause = 0;
        int minimumTilesPerFrame = (int)(layer.tilesInThisLayer.Count * 0.25f);

        // Copy wall
        foreach (SampleTerrain.Layer.Tile tile in layer.tilesInThisLayer)
        {
            DateTime before = DateTime.Now;

            IsGenerating = true;

            // Position of the new tile
            Vector2Int newTilePos = entry + new Vector2Int(invert * tile.position.x, tile.position.y);
            TileBase newTileType = tile.tileType;

            // Check if we need to flip the tile type
            if (invert < 0)
            {
                // Check each tile that can be swapped 
                foreach ((TileBase, TileBase) t in sampleTerrainManager.tilesToSwapWhenInverted)
                {
                    if (tile.tileType.Equals(t.Item1))
                    {
                        newTileType = t.Item2;
                        break;
                    }
                    else if (tile.tileType.Equals(t.Item2))
                    {
                        newTileType = t.Item1;
                        break;
                    }
                }
            }

            // Set the tile
            SetTile(tilemap, newTileType, newTilePos);

            if (loadInBackground)
            {
                // Calculate the time since the last coroutine return 
                DateTime after = DateTime.Now;
                currentFrameTime += (float)(after - before).TotalMilliseconds;

                tilesSinceLastPause++;

                // If the time exceeds the frame time, then call a break
                if (currentFrameTime >= Time.deltaTime && tilesSinceLastPause >= minimumTilesPerFrame)
                {
                    currentFrameTime = 0;
                    tilesSinceLastPause = 0;
                    yield return null;
                }
            }
        }

        IsGenerating = false;

        // Don't need this atm as tiles auto update
        //tilemap.RefreshAllTiles();
    }


    private void CopySampleTerrainLayer(Vector2Int entryPosition, bool flipAxisX, SampleTerrain.Layer layer, Tilemap tilemap)
    {
        IsGenerating = true;

        Vector3Int entry = new Vector3Int(entryPosition.x, entryPosition.y, 0);

        int invert = 1;
        if (flipAxisX)
        {
            invert = -1;
        }

        TileBase[] tileTypes = new TileBase[layer.tilesInThisLayer.Count];
        Vector3Int[] tilePositions = new Vector3Int[layer.tilesInThisLayer.Count];

        // Copy this layer
        for (int i = 0; i < layer.tilesInThisLayer.Count; i++)
        {
            // Position and type of the new tile
            Vector3Int newTilePos = entry + new Vector3Int(invert * layer.tilesInThisLayer[i].position.x, layer.tilesInThisLayer[i].position.y, 0);
            TileBase newTileType = layer.tilesInThisLayer[i].tileType;

            // Check if we need to flip the tile type
            if (invert < 0)
            {
                // Check each tile that can be swapped 
                foreach ((TileBase, TileBase) t in sampleTerrainManager.tilesToSwapWhenInverted)
                {
                    if (layer.tilesInThisLayer[i].tileType.Equals(t.Item1))
                    {
                        newTileType = t.Item2;
                        break;
                    }
                    else if (layer.tilesInThisLayer[i].tileType.Equals(t.Item2))
                    {
                        newTileType = t.Item1;
                        break;
                    }
                }
            }

            // Add the tile to array
            tileTypes[i] = newTileType;
            tilePositions[i] = newTilePos;
        }

        // Set the tiles all in one go
        SetTileCollection(tilemap, tileTypes, tilePositions);

        IsGenerating = false;
    }


    private TerrainChunk GenerateTerrainChunk(Vector2Int entryTile, bool flipAxisX, Direction directionToGenerate, SampleTerrain terrain, Vector2Int chunkID)
    {
        // Get the inverse multiplier
        int invert = 1;
        if (flipAxisX)
        {
            invert = -1;
        }

        // Calculate some important values
        SampleTerrain.GroundBounds b = terrain.GetGroundBounds();
        Vector2 entryPositionWorldCorner = grid.CellToWorld(new Vector3Int(entryTile.x, entryTile.y, 0));
        Vector2 entryPositionWorld = entryPositionWorldCorner + (CellSize / 2);

        // Centre position
        Vector2Int centreTile = entryTile + new Vector2Int(b.minTile.x * invert, b.minTile.y) + new Vector2Int(b.boundsTile.x * invert / 2, b.boundsTile.y / 2);
        Vector2 centre = grid.CellToWorld(new Vector3Int(centreTile.x, centreTile.y, 0));

        // Need to add half a cell for odd numbers
        if (b.boundsTile.x % 2 == 1) { centre.x += CellSize.x / 2; }
        if (b.boundsTile.y % 2 == 1) { centre.y += CellSize.y / 2; }

        // Add extra respawn points 
        List<TerrainChunk.Respawn> respawnPoints = new List<TerrainChunk.Respawn>
        {
            new TerrainChunk.Respawn(Payload.Direction.None, new Vector2(entryPositionWorld.x, entryPositionWorld.y + CellSize.y/2))
        };

        // Add all the extra respawn points
        foreach (SampleTerrain.Spawn s in terrain.extraRespawnPoints)
        {
            Payload.Direction dir = s.direction;

            if (invert == -1)
            {
                if (dir == Payload.Direction.Left)
                {
                    dir = Payload.Direction.Right;
                }
                else if (dir == Payload.Direction.Right)
                {
                    dir = Payload.Direction.Left;
                }
            }

            Vector2Int respawnTile = entryTile + new Vector2Int(invert * s.tilePos.x, s.tilePos.y);
            Vector2 spawnPos = (Vector2)grid.CellToWorld(new Vector3Int(respawnTile.x, respawnTile.y, 0)) + CellSize / 2;
            spawnPos.y += CellSize.y / 2;
            respawnPoints.Add(new TerrainChunk.Respawn(dir, spawnPos));
        }

        // Loop through each sample terrain exit
        List<TerrainChunk.Exit> exits = new List<TerrainChunk.Exit>();
        foreach (SampleTerrain.Exit sampleExit in terrain.exitTilePositions)
        {
            // Calculate where the exit should be
            Vector2Int exitTile = entryTile + new Vector2Int(invert * sampleExit.exitPositionRelative.x, sampleExit.exitPositionRelative.y);
            Vector2 exitPositionWorld = grid.CellToWorld(new Vector3Int(exitTile.x, exitTile.y, 0)) + (grid.cellSize / 2);

            Vector2Int newChunkTile = exitTile;
            Vector2Int newChunkID = chunkID;
            Direction newChunkDirection = directionToGenerate;

            // Move by 1 tile in correct direction
            switch (sampleExit.exitDirection)
            {
                // Exit is left
                case SampleTerrain.ExitDirection.Left:
                    newChunkTile.x += flipAxisX ? 1 : -1;
                    newChunkID.x += flipAxisX ? 1 : -1;
                    newChunkDirection = flipAxisX ? Direction.Right : Direction.Left;
                    break;
                // Exit is right
                case SampleTerrain.ExitDirection.Right:
                    newChunkTile.x += !flipAxisX ? 1 : -1;
                    newChunkID.x += !flipAxisX ? 1 : -1;
                    newChunkDirection = !flipAxisX ? Direction.Right : Direction.Left;
                    break;
                // Exit is up
                case SampleTerrain.ExitDirection.Up:
                    newChunkTile.y++;
                    newChunkID.y++;
                    newChunkDirection = Direction.Up;
                    break;
                // Exit is down
                case SampleTerrain.ExitDirection.Down:
                    newChunkTile.y--;
                    newChunkID.y--;
                    newChunkDirection = Direction.Down;
                    break;
            }

            // World position of the start of the new chunk
            Vector2 newChunkPositionWorld = (Vector2)grid.CellToWorld(new Vector3Int(newChunkTile.x, newChunkTile.y, 0)) + (CellSize / 2);

            Vector2Int tilesAwayFromOrigin = exitTile - initialTilePos;

            // Make the exit
            TerrainChunk.Exit e = new TerrainChunk.Exit(newChunkDirection, exitPositionWorld, newChunkPositionWorld, newChunkID, tilesAwayFromOrigin);

            // Get the camera path points
            foreach (Vector2Int point in sampleExit.cameraPathPoints)
            {
                // Get the world pos
                Vector3Int pointTile = new Vector3Int(entryTile.x + invert * point.x, entryTile.y + point.y, 0);
                Vector2 worldPos = (Vector2)grid.CellToWorld(pointTile) + CellSize / 2;

                // Add the point 
                e.cameraPathPoints.Add(worldPos);
            }

            // Sort the lists by distance from entry tile pos
            e.cameraPathPoints.Sort((x, y) => Vector2.Distance(entryPositionWorld, x).CompareTo(Vector2.Distance(entryPositionWorld, y)));

            // Add the exit to list of exits for this chunk
            exits.Add(e);
        }

        List<TerrainChunk.Item> allItemPositions = new List<TerrainChunk.Item>();

        // Calculate the Items
        foreach (SampleTerrain.Item item in terrain.items)
        {
            // Get position of the centre of the tile
            Vector2 pos = (Vector2)grid.CellToWorld(new Vector3Int(entryTile.x + invert * item.tilePos.x, entryTile.y + item.tilePos.y, 0)) + (CellSize / 2);
            // And add it
            allItemPositions.Add(new TerrainChunk.Item(item.name, pos, new Vector2(pos.x, pos.y - CellSize.y / 2)));
        }

        // Get all extra world objects
        List<(GameObject, Vector2, Vector3)> extraWorldObjects = new List<(GameObject, Vector2, Vector3)>();
        for (int i = 0; i < terrain.extraGameObjects.Count; i++)
        {
            // Calculate the new position and rotation
            Vector2 position = entryPositionWorldCorner + (new Vector2(invert * terrain.extraGameObjects[i].Item2.x, terrain.extraGameObjects[i].Item2.y) * CellSize);
            Vector3 localScale = terrain.extraGameObjects[i].Item1.transform.localScale;

            // The object needs to be flipped
            if (invert < 0)
            {
                position.x += CellSize.x;

                localScale.x *= -1;
            }

            extraWorldObjects.Add((terrain.extraGameObjects[i].Item1, position, localScale));
        }

        // Get the finishing bounds if this chunk is a finish
        TerrainChunk.Finish finish = new TerrainChunk.Finish();
        if (terrain.endTileLocations != null)
        {
            if (terrain.endTileLocations.Count > 0)
            {
                Vector2Int defaultValue = entryTile + new Vector2Int(invert * terrain.endTileLocations[0].x, terrain.endTileLocations[0].y);
                Vector2Int minTile = defaultValue, maxTile = defaultValue;
                foreach (Vector2Int exitTile in terrain.endTileLocations)
                {
                    // Set the min and max points
                    Vector2Int tile = entryTile + new Vector2Int(invert * exitTile.x, exitTile.y);
                    minTile.x = tile.x < minTile.x ? tile.x : minTile.x;
                    minTile.y = tile.y < minTile.y ? tile.y : minTile.y;

                    maxTile.x = tile.x > maxTile.x ? tile.x : maxTile.x;
                    maxTile.y = tile.y > maxTile.y ? tile.y : maxTile.y;
                }

                Vector2 min = grid.CellToWorld(new Vector3Int(minTile.x, minTile.y, 0));
                Vector2 max = (Vector2)grid.CellToWorld(new Vector3Int(maxTile.x, maxTile.y, 0)) + CellSize;
                Vector2 size = max - min;

                Payload.Direction direction = Payload.Direction.None;
                if (chunkID.x < ChunkManager.initialChunkID.x)
                {
                    direction = Payload.Direction.Left;
                }
                else if (chunkID.x > ChunkManager.initialChunkID.x)
                {
                    direction = Payload.Direction.Right;
                }

                finish = new TerrainChunk.Finish(new Bounds(min + (size / 2), size), direction);
            }
        }


        // Return the TerrainChunk object for use in the ChunkManager
        return new TerrainChunk(b.boundsReal, CellSize, centre, entryPositionWorld, exits, respawnPoints, directionToGenerate, chunkID,
            allItemPositions, terrain.itemChance, extraWorldObjects, finish, terrain.index);
    }



    private bool IsValidDirection(Direction directionToGenerate, Direction sampleTerrainDirection)
    {
        // Terrain is already the correct direction
        return directionToGenerate.Equals(sampleTerrainDirection)
            // Terrain can go both ways
            || (directionToGenerate.Equals(Direction.Both) && (sampleTerrainDirection.Equals(Direction.Left) || sampleTerrainDirection.Equals(Direction.Right)))
            // Terrain will need to be flipped
            || (directionToGenerate.Equals(Direction.Left) && sampleTerrainDirection.Equals(Direction.Right))
            || (directionToGenerate.Equals(Direction.Right) && sampleTerrainDirection.Equals(Direction.Left));
    }

    public void ClearAllTiles()
    {
        wall.ClearAllTiles();
        wallDetail.ClearAllTiles();
        background.ClearAllTiles();
        ground.ClearAllTiles();
        hazard.ClearAllTiles();
    }


    public Vector2 GetInitialTileWorldPositionForPlayer()
    {
        // Get the initial position + (half a cell, 1 cell, 0) to point to the top, middle of the cell
        return ground.CellToWorld(new Vector3Int(initialTilePos.x, initialTilePos.y, ground.cellBounds.z)) + new Vector3(ground.cellSize.x / 2, ground.cellSize.y, 0);
    }


    private Vector2Int GenerateInitialTile(Vector2Int tilePos)
    {
        // Calculate the initial position
        SetTile(ground, groundTile, tilePos);

        return tilePos;
    }


    private void SetTileCollection(Tilemap tilemap, TileBase[] tileTypes, Vector3Int[] tilePositions)
    {
        // Pass an array of tiles to be set
        // This is insanely better for performance
        tilemap.SetTiles(tilePositions, tileTypes);
    }

    private void SetTile(Tilemap tilemap, TileBase tileType, Vector2Int tilePosition)
    {
        // Set the cell
        tilemap.SetTile(new Vector3Int(tilePosition.x, tilePosition.y, 0), tileType);
    }


    public Vector2Int GetGroundBoundsTiles()
    {
        Vector3Int bounds = ground.cellBounds.size;
        return new Vector2Int(bounds.x, bounds.y);
    }

    public Vector2Int GetTilePosition(Vector2 position)
    {
        Vector3Int tile = grid.WorldToCell(position);
        return new Vector2Int(tile.x, tile.y);
    }


    public SampleTerrain GetSampleTerrain(int sampleTerrainID)
    {
        return sampleTerrainManager.allSamples[sampleTerrainID];
    }


    public enum Generation
    {
        Multidirectional_Endless,
        Symmetrical_Endless,
        Symmetrical_Limit,
    }


    /// <summary>
    /// The direction that the terrain is facing. Camera should move in that direction.
    /// </summary>
    public enum Direction
    {
        Left,
        Right,
        Up,
        Down,
        Both
    }


    /// <summary>
    /// Class between Sample Terrain and Chunk.  Used to store values when passed as an event.
    /// </summary>
    public struct TerrainChunk
    {
        public Vector2 bounds;
        public Vector2 cellSize;
        public Vector2 centre;

        public Vector2 enteranceWorldPosition;
        public List<Exit> exits;
        public List<Respawn> respawnPoints;

        public Direction direction;
        public Vector2Int chunkID;

        public List<Item> items;
        public float itemChance;

        public List<(GameObject, Vector2, Vector3)> extraWorldObjects;

        public int sampleIndex;

        public Finish finishArea;

        public TerrainChunk(Vector2 bounds, Vector2 cellSize, Vector2 centre, Vector2 enteranceWorldPosition, List<Exit> exits, List<Respawn> respawnPoints,
            Direction direction, Vector2Int chunkID, List<Item> items, float itemChance, List<(GameObject, Vector2, Vector3)> extraWorldObjects, Finish finishArea,
            int sampleIndex)
        {
            this.bounds = bounds;
            this.cellSize = cellSize;
            this.centre = centre;
            this.enteranceWorldPosition = enteranceWorldPosition;
            this.exits = exits;
            this.respawnPoints = respawnPoints;
            this.direction = direction;
            this.chunkID = chunkID;
            this.items = items;
            this.itemChance = itemChance;
            this.extraWorldObjects = extraWorldObjects;
            this.finishArea = finishArea;
            this.sampleIndex = sampleIndex;
        }

        public struct Exit
        {
            public Direction exitDirection;
            public Vector2 exitPositionWorld;

            public Vector2 newChunkPositionWorld;
            public Vector2Int newChunkID;

            public List<Vector2> cameraPathPoints;

            public Vector2Int tilesFromOrigin;

            public Exit(Direction exitDirection, Vector2 exitPositionWorld, Vector2 newChunkPositionWorld, Vector2Int newChunkID, Vector2Int tilesFromOrigin)
            {
                this.exitDirection = exitDirection;
                this.exitPositionWorld = exitPositionWorld;
                this.newChunkPositionWorld = newChunkPositionWorld;
                this.newChunkID = newChunkID;

                cameraPathPoints = new List<Vector2>();

                this.tilesFromOrigin = tilesFromOrigin;
            }
        }

        public struct Respawn
        {
            public Payload.Direction direction;
            public Vector2 position;

            public Respawn(Payload.Direction direction, Vector2 position)
            {
                this.direction = direction;
                this.position = position;
            }
        }


        public struct Item
        {
            public string name;
            public Vector2 centreOfTile;
            public Vector2 groundPosition;

            public Item(string name, Vector2 centreOfTile, Vector2 groundPosition)
            {
                this.name = name;
                this.centreOfTile = centreOfTile;
                this.groundPosition = groundPosition;
            }
        }


        public struct Finish
        {
            public bool isFinish;
            public Bounds bounds;
            public Payload.Direction direction;


            public Finish(Bounds bounds, Payload.Direction direction)
            {
                isFinish = true;

                this.bounds = bounds;
                this.direction = direction;
            }
        }
    }



}


