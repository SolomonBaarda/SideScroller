using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractionManager : MonoBehaviour
{
    public static UnityAction<GameObject, PlayerInventory> OnPlayerInteractWithItem;

    private void Awake()
    {
        OnPlayerInteractWithItem += InteractWithItem;
    }

    private void OnDestroy()
    {
        OnPlayerInteractWithItem -= InteractWithItem;
    }


    private void InteractWithItem(GameObject item, PlayerInventory inventory)
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
            // Interct with it last
            IInteractable interactable = (IInteractable)WorldItem.GetClass<IInteractable>(item);
            try
            {
                interactable.Interact(inventory);
            }
            // The PlayerInventory could be null if the object didn't have access to it
            catch (System.NullReferenceException)
            {
                Debug.LogError("Item interact called on item " + item.name + " with null PlayerInventory.");
            }

        }
    }

    private void CollideWithItem(GameObject item, GameObject player)
    {
        // TODO
    }



}
