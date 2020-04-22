using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class ItemManager : MonoBehaviour
{
    public static UnityAction<InteractableItem, Vector2> OnPlayerInteractWithItem;

    private System.Random random;

    public Dictionary<Item, GameObject> itemPrefabs;

    public static string ITEM_LAYER = "Item";
    public static string ITEM_CAN_PICK_UP_TAG = "ITEM_CanPickUp";
    public static string ITEM_INTERACTABLE_TAG = "ITEM_Interactable";

    private void Awake()
    {
        itemPrefabs = new Dictionary<Item, GameObject>();
        LoadAllItemPrefabs(ref itemPrefabs);
        //Debug.Log("item prefab size " + itemPrefabs.Count);

        TerrainManager.OnTerrainChunkGenerated += GenerateItemsForChunk;
        OnPlayerInteractWithItem += InteractWithItem;

        random = new System.Random(0);
    }





    private void InteractWithItem(InteractableItem item, Vector2 position)
    {
        LootTable table = item.GetLootTable();
        int value = random.Next(0, table.GetTotalWeight());
        Item drop = table.GetLoot(value);

        GameObject g;
        itemPrefabs.TryGetValue(drop, out g);

        if (item.Interact())
        {
            SpawnItem(g, position, drop.ToString());
        }

    }



    private void LoadAllItemPrefabs(ref Dictionary<Item, GameObject> prefabs)
    {
        // Load all item prefabs from "Assets/Resources/ItemPrefabs"
        GameObject[] allItemPrefabs = Resources.LoadAll<GameObject>("Prefabs/Items");

        // Loop through each object
        foreach (GameObject g in allItemPrefabs)
        {
            // Try and get the enum type from the name
            Item type;
            if (!Enum.TryParse(g.name, out type))
            {
                Debug.LogError("Could not parse prefab " + g.name + " to Item enum.");
                continue;
            }

            // Add it
            prefabs.Add(type, g);
        }
    }


    private void GenerateItemsForChunk(TerrainManager.TerrainChunk chunk)
    {
        float itemChance = chunk.itemChance;

        // Loop through each item position
        foreach (TerrainManager.TerrainChunk.Item item in chunk.items)
        {
            GameObject prefab;
            if (itemPrefabs.TryGetValue(item.itemType, out prefab))
            {
                if (random.Next(0, 1) <= itemChance)
                {
                    SpawnItem(prefab, item.centreOfTile, item.itemType.ToString());
                }
            }
        }
    }

    private void SpawnItem(GameObject item, Vector2 position, string name)
    {
        GameObject g = Instantiate(item, position, Quaternion.identity, transform);
        g.layer = LayerMask.NameToLayer(ITEM_LAYER);
        g.name = name;
    }




    public enum Item
    {
        Coin,
        Pot,
        Chest
    }
}
