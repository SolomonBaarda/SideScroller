using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldItem : MonoBehaviour
{
    public Sprite sprite;
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D boxCollider;

    public Item contents;


    private void Awake()
    {
        // Set the sprite 
        if (contents != null)
        {
            sprite = contents.sprite;
        }

        // Set the layer
        gameObject.layer = LayerMask.NameToLayer(ItemManager.ITEM_LAYER);

        // Set up the sprite renderer
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        //spriteRenderer.sortingLayerName = ItemManager.ITEM_LAYER;

        // Set up the box collider
        boxCollider = gameObject.AddComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
    }



}
