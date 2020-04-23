using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractionManager : MonoBehaviour
{

    public static UnityAction<GameObject> OnPlayerInteractWithItem;


    private void Awake()
    {
        OnPlayerInteractWithItem += InteractWithItem;
    }


    private void InteractWithItem(GameObject item)
    {
        // Ensure it is a valid interaction
        if (WorldItem.ImplementsInterface<IInteractable>(item))
        {
            // It is loot that needs to be picked up
            if (WorldItem.ImplementsInterface<ILoot>(item))
            {

            }
            // Check if it is lootable
            else if (WorldItem.ImplementsInterface<ILootable>(item))
            {
                ItemManager.OnGenerateLoot.Invoke(item);
            }
        }
    }

    private void CollideWithItem(GameObject item, GameObject player)
    {
        // TODO
    }



}
