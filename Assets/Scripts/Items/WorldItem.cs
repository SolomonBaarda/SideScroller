using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldItem : MonoBehaviour
{
    [SerializeField]
    public Item item;
    [Space]


    public Sprite sprite;
    protected SpriteRenderer spriteRenderer;
    protected BoxCollider2D boxCollider;

    public Name itemName;

    protected void Awake()
    {
        // See if item needs to be updated
        if (item != null)
        {
            // Set the sprite if not already set
            if (sprite == null)
            {
                sprite = item.sprite;
            }

            // Update the name if not already done
            if (item.display_name == null)
            {
                item.display_name = itemName.ToString();
            }
        }

        // Set the layer
        gameObject.layer = LayerMask.NameToLayer(ItemManager.ITEM_LAYER);

        // Set up the sprite renderer
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;

        // Set up the box collider
        boxCollider = gameObject.AddComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
    }


    protected void SetRendererSortingLayer(string layer)
    {
        spriteRenderer.sortingLayerID = SortingLayer.NameToID(layer);
    }



    public static bool ImplementsInterface<T>(GameObject toCheck) where T : class
    {
        return GetScriptThatImplements<T>(toCheck) != null;
    }


    public static MonoBehaviour GetScriptThatImplements<T>(GameObject toCheck) where T : class
    {
        // Get all monobehaviours 
        MonoBehaviour[] all = toCheck.GetComponents<MonoBehaviour>();

        // Loop through each
        foreach (MonoBehaviour behaviour in all)
        {
            // If the monobehaviour implements the interface
            if (behaviour is T)
            {
                // Return it
                return behaviour;
            }
        }

        return null;
    }



    /// <summary>
    /// A list of all possible world items.
    /// </summary>
    public enum Name
    {
        Coin,
        Pot,
        Chest
    }

}
