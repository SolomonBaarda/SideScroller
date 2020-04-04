using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunks : MonoBehaviour
{
    public List<TerrainChunk> chunks;


    [Serializable]
    public class TerrainChunk
    {
        public Terrain terrain = new Terrain();
        public Vector2Int entryPosition;
        public Vector2Int exitPosition;
    }
}
