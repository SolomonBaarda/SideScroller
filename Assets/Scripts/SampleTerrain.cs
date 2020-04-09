﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SampleTerrain : MonoBehaviour
{
    private Grid grid;
    private Tilemap tilemap_wall;
    private Tilemap tilemap_wallDetail;
    private Tilemap tilemap_background;
    private Tilemap tilemap_ground;
    private Tilemap tilemap_dev;

    private SampleTerrainManager manager;

    public SampleTerrainLayer wall;
    public SampleTerrainLayer wallDetail;
    public SampleTerrainLayer background;
    public SampleTerrainLayer ground;

    public Vector2Int entryTilePosition;
    private Vector2Int entryTilePositionLocal;
    public Vector2Int exitTilePosition;

    private void Awake()
    {
        manager = transform.parent.GetComponent<SampleTerrainManager>();
        grid = GetComponent<Grid>();

        for (int i = 0; i < grid.transform.childCount; i++)
        {
            GameObject g = grid.transform.GetChild(i).gameObject;
            Tilemap t = g.GetComponent<Tilemap>();
            TilemapRenderer r = g.GetComponent<TilemapRenderer>();

            if (r.sortingLayerName.Equals(TerrainManager.LAYER_NAME_WALL))
            {
                tilemap_wall = t;
            }
            else if (r.sortingLayerName.Equals(TerrainManager.LAYER_NAME_WALL_DETAIL))
            {
                tilemap_wallDetail = t;
            }
            else if (r.sortingLayerName.Equals(TerrainManager.LAYER_NAME_BACKGROUND))
            {
                tilemap_background = t;
            }
            else if (r.sortingLayerName.Equals(TerrainManager.LAYER_NAME_GROUND))
            {
                tilemap_ground = t;
            }
            else if (r.sortingLayerName.Equals(TerrainManager.LAYER_NAME_DEV))
            {
                tilemap_dev = t;
            }

            // Disable the rendering of all samples
            r.enabled = false;
        }

        entryTilePositionLocal = FindEntryTilePosition();
        entryTilePosition = Vector2Int.zero;
        exitTilePosition = FindExitTilePosition();

        wall = new SampleTerrainLayer(TerrainManager.LAYER_NAME_WALL);
        wallDetail = new SampleTerrainLayer(TerrainManager.LAYER_NAME_WALL_DETAIL);
        background = new SampleTerrainLayer(TerrainManager.LAYER_NAME_BACKGROUND);
        ground = new SampleTerrainLayer(TerrainManager.LAYER_NAME_GROUND);

        // Load all the tiles in the tilemaps into the objects
        LoadTiles(tilemap_wall, ref wall);
        LoadTiles(tilemap_wallDetail, ref wallDetail);
        LoadTiles(tilemap_background, ref background);
        LoadTiles(tilemap_ground, ref ground);
    }


    public Vector3 GetTileSize()
    {
        return grid.cellSize;
    }

    public Vector2 GetGroundBounds()
    {
        // Get array just so we can initialise the variables to the first element 
        SampleTerrainLayer.SampleTerrainTile[] tiles = ground.tilesInThisLayer.ToArray();

        Vector2Int min = tiles[0].position, max = tiles[0].position;

        // Might as well use the array since we have it lol
        foreach(SampleTerrainLayer.SampleTerrainTile tile in tiles)
        {
            if(tile.position.x < min.x)
            {
                min.x = tile.position.x;
            }
            if (tile.position.y < min.y)
            {
                min.y = tile.position.y;
            }

            if (tile.position.x > max.x)
            {
                max.x = tile.position.x;
            }
            if (tile.position.y > max.y)
            {
                max.y = tile.position.y;
            }
        }

        // Add one to the difference as tilemap is 0 based
        return max - min + Vector2.one;
    }



    private void LoadTiles(Tilemap tilemap, ref SampleTerrainLayer layer)
    {
        // Get an iterator for the bounds of the tilemap 
        BoundsInt.PositionEnumerator p = tilemap.cellBounds.allPositionsWithin.GetEnumerator();
        while (p.MoveNext())
        {
            Vector3Int current = p.Current;
            // Get the tile
            Tile t = (Tile)tilemap.GetTile(current);
            if (t != null)
            {
                // Add it to the list of tiles for that layer
                layer.tilesInThisLayer.Add(new SampleTerrainLayer.SampleTerrainTile(t, new Vector2Int(current.x, current.y) - entryTilePositionLocal));
            }
        }
    }


    private Vector2Int FindEntryTilePosition()
    {
        // Get an iterator for the bounds of the tilemap 
        BoundsInt.PositionEnumerator p = tilemap_dev.cellBounds.allPositionsWithin.GetEnumerator();
        while (p.MoveNext())
        {
            Vector3Int current = p.Current;
            if (tilemap_dev.GetTile(current) != null)
            {
                // Check if the tile matches the entry tile type
                if (tilemap_dev.GetTile(current).Equals(manager.dev_entryTile))
                {
                    return new Vector2Int(current.x, current.y);
                }
            }
        }
        throw new Exception("Entry tile could not be found in SampleTerrain.");
    }

    // Must be called AFTER entry position has been assigned
    private Vector2Int FindExitTilePosition()
    {
        // Get an iterator for the bounds of the tilemap 
        BoundsInt.PositionEnumerator p = tilemap_dev.cellBounds.allPositionsWithin.GetEnumerator();
        while (p.MoveNext())
        {
            Vector3Int current = p.Current;
            if (tilemap_dev.GetTile(current) != null)
            {
                // Check if the tile matches the entry tile type
                if (tilemap_dev.GetTile(current).Equals(manager.dev_exitTile))
                {
                    return (new Vector2Int(current.x, current.y) - entryTilePositionLocal);
                }
            }
        }
        throw new Exception("Exit tile could not be found in SampleTerrain.");
    }


    public class SampleTerrainLayer
    {
        public string layer;
        public List<SampleTerrainTile> tilesInThisLayer;

        public SampleTerrainLayer(string layer)
        {
            this.layer = layer;
            tilesInThisLayer = new List<SampleTerrainTile>();
        }

        public class SampleTerrainTile
        {
            public SampleTerrainTile(Tile tileType, Vector2Int position)
            {
                this.tileType = tileType;
                this.position = position;

            }
            public Tile tileType;
            public Vector2Int position;
        }
    }



}
