using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldItem : MonoBehaviour
{
    [SerializeField]
    public Item item;
    public Name itemName;
    [Space]

    [SerializeField]
    protected Sprite sprite;
    protected SpriteRenderer spriteRenderer;
    [Space]
    protected BoxCollider2D boxCollider;

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
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sprite = sprite;

        // Set up the box collider
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        boxCollider.isTrigger = true;
    }


    protected void SetRendererSortingLayer(string layer)
    {
        spriteRenderer.sortingLayerID = SortingLayer.NameToID(layer);
    }


    /// <summary>
    /// Returns true if the attached GameObject has a script that implements the interface T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="toCheck"></param>
    /// <returns></returns>
    public static bool ImplementsInterface<T>(GameObject toCheck) where T : class
    {
        return GetScriptThatImplements<T>(toCheck) != null;
    }


    /// <summary>
    /// Returns the script that implements interface T in GameObject. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="toCheck"></param>
    /// <returns></returns>
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
