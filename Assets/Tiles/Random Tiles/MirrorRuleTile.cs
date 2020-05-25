using UnityEngine;
using UnityEngine.Tilemaps;
using System;


[Serializable]
[CreateAssetMenu]
public class MirrorRuleTile : RuleTile
{
    public RuleTile left, right;


    public override void RefreshTile(Vector3Int location, ITilemap tilemap)
    {
        base.RefreshTile(location, tilemap);

        TileBase l = tilemap.GetTile(new Vector3Int(location.x - 1, location.y, location.z));
        TileBase r = tilemap.GetTile(new Vector3Int(location.x + 1, location.y, location.z));
        TileBase u = tilemap.GetTile(new Vector3Int(location.x, location.y + 1, location.z));
        TileBase d = tilemap.GetTile(new Vector3Int(location.x, location.y - 1, location.z));

        // There is only a tile on the left
        if (l != null && r == null)
        {
            if (l is MirrorRuleTile)
            {
                if (u != null)
                {
                    if (u is MirrorRuleTile)
                    {
                        m_TilingRules = left.m_TilingRules;
                        m_DefaultSprite = left.m_DefaultSprite;
                        UpdateNeighborPositions();
                    }
                }
            }
            else
            {

            }
        }



        base.RefreshTile(location, tilemap);
    }


}
