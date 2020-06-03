using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractionManager : MonoBehaviour
{
    public static UnityAction<GameObject> OnInteractWithItem;

    private void Awake()
    {
        OnInteractWithItem += InteractWithItem;
    }

    private void OnDestroy()
    {
        OnInteractWithItem -= InteractWithItem;
    }


    private void InteractWithItem(GameObject item)
    {
        // Ensure it is a valid interaction
        if (WorldItem.ExtendsClass<IInteractable>(item))
        {
            // It is loot that needs to be picked up
            if (WorldItem.ExtendsClass<ILoot>(item))
            {

            }
            // Check if it is lootable
            if (WorldItem.ExtendsClass<ILootable>(item))
            {
                ILootable lootable = (ILootable)WorldItem.GetClass<ILootable>(item);

                // Ensure it is not empty
                if (lootable.IsLootable())
                {
                    // Generate loot
                    ItemManager.OnGenerateLoot.Invoke(item);
                }
            }
        }
    }


}
