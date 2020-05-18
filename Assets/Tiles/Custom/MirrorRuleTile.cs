using UnityEngine;
using UnityEngine.Tilemaps;


public class MirrorRuleTile : Tile
{
    public RuleTile left, right;


    public override void RefreshTile(Vector3Int location, ITilemap tilemap)
    {
        base.RefreshTile(location, tilemap);

        TileBase l = tilemap.GetTile(new Vector3Int(location.x-1, location.y, location.z));
        TileBase r = tilemap.GetTile(new Vector3Int(location.x+1, location.y, location.z));

        // Tiles on both sides
        if(l != null && r != null)
        {

        }
        // Tile only on the left
        else if(l != null && r == null)
        {

        }
        // Tile only on the right 
        else if (l == null && r != null)
        {

        }
        // No tiles left or right
        else
        {

        }
    }


}
