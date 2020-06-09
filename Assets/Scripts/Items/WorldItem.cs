using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldItem : MonoBehaviour
{
    public const int DEFAULT_MAX_OVERLAP_COLLISIONS = 8;

    public ItemBase item;
    [Space]

    [SerializeField]
    protected Sprite sprite;
    protected SpriteRenderer spriteRenderer;
    [Space]
    protected BoxCollider2D trigger;

    public virtual void Awake()
    {
        // See if item needs to be updated
        if (item != null)
        {
            // Set the sprite if not already set
            if (item.sprite != null)
            {
                sprite = item.sprite;
            }
        }

        // Set the layer
        gameObject.layer = LayerMask.NameToLayer(ItemManager.ITEM_LAYER);

        // Set up the sprite renderer
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            else
            {
                // Check if it has a sprite
                if(spriteRenderer.sprite != null)
                {
                    sprite = spriteRenderer.sprite;
                }
            }
        }

        spriteRenderer.sprite = sprite;

        // Set up the box collider
        if (trigger == null)
        {
            trigger = GetComponent<BoxCollider2D>();
            if (trigger == null)
            {
                trigger = gameObject.AddComponent<BoxCollider2D>();
            }
        }

        trigger.isTrigger = true;
    }


    /// <summary>
    /// Returns true if the attached GameObject has a script that implements the class T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="toCheck"></param>
    /// <returns></returns>
    public static bool ExtendsClass<T>(GameObject toCheck) where T : class
    {
        return GetClass<T>(toCheck) != null;
    }


    /// <summary>
    /// Returns the monobehaviour that implements class T in GameObject. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="toCheck"></param>
    /// <returns></returns>
    public static MonoBehaviour GetClass<T>(GameObject toCheck) where T : class
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


    public static IEnumerator WaitForHazardCollisionThenDestroy(GameObject item)
    {
        Collider2D[] allColliders = item.GetComponents<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask(Hazard.LAYER));
        bool hasCollided = false;

        while(!hasCollided)
        {
            // Check each collider
            foreach (Collider2D c in allColliders)
            {
                // Check if there has been at least one collision
                if(Physics2D.OverlapCollider(c, filter, new Collider2D[1]) > 0)
                {
                    // Destroy the GameObject and exit
                    hasCollided = true;
                    Destroy(item);
                    yield break;
                }
            }

            yield return null;
        }
    }


}
