using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class ItemManager : MonoBehaviour
{
    public static UnityAction<Vector2> OnPotBroken;
    public static UnityAction<Vector2> OnChestOpened;

    private System.Random random;

    public GameObject g;
    public Dictionary<Item, GameObject> itemPrefabs;


    private void Awake()
    {
        itemPrefabs = new Dictionary<Item, GameObject>();
        LoadAllItemPrefabs(ref itemPrefabs);
        //Debug.Log("item prefab size " + itemPrefabs.Count);

        TerrainManager.OnTerrainChunkGenerated += GenerateItemsForChunk;

        random = new System.Random(0);
    }

    private void LoadAllItemPrefabs(ref Dictionary<Item, GameObject> prefabs)
    {
        // Load all item prefabs from "Assets/Resources/ItemPrefabs"
        GameObject[] allItemPrefabs = Resources.LoadAll<GameObject>("ItemPrefabs");

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
                if(random.Next(0, 1) <= itemChance)
                {
                    SpawnItem(prefab, item.centreOfTile, item.itemType.ToString());
                }
            }
        }
    }

    private void SpawnItem(GameObject item, Vector2 position, string name)
    {
        GameObject g = Instantiate(item, position, Quaternion.identity, transform);
        g.name = name;
    }




    private void SpawnRandomLoot(Vector2 pos, GameObject[] loot)
    {
        int index = Mathf.FloorToInt(random.Next(0, loot.Length));


    }


    private void PotBroken(Vector2 pos)
    {
        //SpawnRandomLoot(pos, lootTablePrefabs);
    }

    private void ChestOpened(Vector2 pos)
    {
        //SpawnRandomLoot(pos, lootTablePrefabs);
    }



    public enum Item
    {
        Coin,
        Pot,
        Chest
    }
}
