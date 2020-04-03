using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class Chunk
{
    public List<Tile> groundTiles;
    public Vector2Int entryPosition;
    public Vector2Int exitPosition;
}
