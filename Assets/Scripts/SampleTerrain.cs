using System;
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

    [Header("Dev tile textures")]
    public Tile entryTileType;

    public SampleTerrainLayer wall;
    public SampleTerrainLayer wallDetail;
    public SampleTerrainLayer background;
    public SampleTerrainLayer ground;

    public Vector2Int entryTilePosition;
    public Vector2Int exitTilePosition;

    private int width, height;

    private Vector2Int topLeftRelativeToEntry;
    private Vector2Int bottomRightRelativeToEntry;

    private void Awake()
    {
        grid = GetComponent<Grid>();

        for (int i = 0; i < grid.transform.childCount; i++)
        {
            GameObject g = grid.transform.GetChild(i).gameObject;
            Tilemap t = g.GetComponent<Tilemap>();
            TilemapRenderer r = g.GetComponent<TilemapRenderer>();

            if (r.sortingLayerName.Equals("Wall"))
            {
                tilemap_wall = t;
            }
            else if (r.sortingLayerName.Equals("Wall Detail"))
            {
                tilemap_wallDetail = t;
            }
            else if (r.sortingLayerName.Equals("Background"))
            {
                tilemap_background = t;
            }
            else if (r.sortingLayerName.Equals("Ground"))
            {
                tilemap_ground = t;
            }
            else if (r.sortingLayerName.Equals("Dev"))
            {
                tilemap_dev = t;
            }
        }


        entryTilePosition = FindEntryTilePosition();
    }


    private void LoadTiles(Tilemap tilemap, ref SampleTerrainLayer layer)
    {
        // Get an iterator for the bounds of the tilemap 
        BoundsInt.PositionEnumerator p = tilemap.cellBounds.allPositionsWithin.GetEnumerator();
        while (p.MoveNext())
        {
            Vector3Int current = p.Current;
            if (tilemap.GetTile(current) != null)
            {
                layer

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
                if (tilemap_dev.GetTile(current).Equals(entryTileType))
                {
                    return new Vector2Int(current.x, current.y);
                }
            }
        }

        throw new Exception("Tile entry position could not be found.");
    }


    public class SampleTerrainLayer
    {
        String layer;
        List<SampleTerrainTileType> tilesInThisLayer;

        public SampleTerrainLayer(String layer)
        {
            this.layer = layer; 
        }

        public class SampleTerrainTileType
        {
            Tile tileType;
            // List of tile positions (relative to the entry tile position)
            List<Vector2Int> positions;
        }
    }



}
