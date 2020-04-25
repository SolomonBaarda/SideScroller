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


    private void InteractWithItem(GameObject item, PlayerInventory inventory)
    {
        // Ensure it is a valid interaction
        if (WorldItem.ImplementsInterface<IInteractable>(item))
        {
            // It is loot that needs to be picked up
            if (WorldItem.ImplementsInterface<ILoot>(item))
            {

            }
            // Check if it is lootable
            if (WorldItem.ImplementsInterface<ILootable>(item))
            {
                ILootable lootable = (ILootable)WorldItem.GetScriptThatImplements<ILootable>(item);

                // Ensure it is not empty
                if (lootable.IsLootable())
                {
                    // Generate loot
                    ItemManager.OnGenerateLoot.Invoke(item);
                }
            }


            // Interct with it last
            IInteractable interactable = (IInteractable)WorldItem.GetScriptThatImplements<IInteractable>(item);
            interactable.Interact(inventory);
        }
    }

    private void CollideWithItem(GameObject item, GameObject player)
    {
        // TODO
    }



}
