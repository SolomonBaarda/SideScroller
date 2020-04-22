using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class ItemManager : MonoBehaviour
{
    public static string ITEM_LAYER = "Item";
    public static UnityAction<InteractableItem, Vector2> OnPlayerInteractWithItem;

    private System.Random random;

    private Dictionary<Loot, GameObject> lootPrefabs;
    private Dictionary<InteractableItem.Name, GameObject> interactableItemWorldObjectPrefabs;
    private Dictionary<Weapon.Name, GameObject> weaponScriptableObjectPrefabs;
    private Dictionary<Buff.Name, GameObject> buffScriptableObjectPrefabs;

    public GameObject worlditemPrefab;

    private void Awake()
    {
        DateTime before = DateTime.Now;

        lootPrefabs = new Dictionary<Loot, GameObject>();
        interactableItemWorldObjectPrefabs = new Dictionary<InteractableItem.Name, GameObject>();
        weaponScriptableObjectPrefabs = new Dictionary<Weapon.Name, GameObject>();
        buffScriptableObjectPrefabs = new Dictionary<Buff.Name, GameObject>();

        // Load all items 
        LoadAllItemPrefabs(ref interactableItemWorldObjectPrefabs, "Prefabs/Items");
        LoadAllItemPrefabs(ref weaponScriptableObjectPrefabs, "Scripts/Weapons");
        LoadAllItemPrefabs(ref buffScriptableObjectPrefabs, "Scripts/Buffs");

        DateTime after = DateTime.Now;
        TimeSpan time = after - before;

        Debug.Log("Loaded items in " + time.Milliseconds + "ms: (" + interactableItemWorldObjectPrefabs.Count + " interactable items), (" + lootPrefabs.Count + " loot), (" + buffScriptableObjectPrefabs.Count + " buffs), (" +
                weaponScriptableObjectPrefabs.Count + " weapons)");

        TerrainManager.OnTerrainChunkGenerated += GenerateItemsForChunk;
        OnPlayerInteractWithItem += InteractWithItem;

        random = new System.Random(0);
    }





    private void InteractWithItem(InteractableItem item, Vector2 position)
    {
        LootTable table = item.GetLootTable();
        int value = random.Next(0, table.GetTotalWeight());
        Loot drop = table.GetLoot(value);

        GameObject g;
        lootPrefabs.TryGetValue(drop, out g);

        if (item.Interact())
        {
            SpawnItem(g, position, drop.ToString());
        }
    }



    /// <summary>
    /// Load all prefabs from "path" and put them into a Dictionary
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="prefabs"></param>
    /// <param name="path"></param>
    private void LoadAllItemPrefabs<TEnum>(ref Dictionary<TEnum, GameObject> prefabs, string path) where TEnum : struct
    {
        // Load all objects from path
        GameObject[] allItemPrefabs = Resources.LoadAll<GameObject>(path);

        // Loop through each object
        foreach (GameObject g in allItemPrefabs)
        {
            // Try and get the enum type from the name
            TEnum type;
            if (!Enum.TryParse(g.name, out type))
            {
                Debug.LogError("Could not parse prefab " + g.name + " to enum type.");
                continue;
            }

            // Add it
            prefabs.Add(type, g);


            // Check if it is a loot item, and if so add it
            Loot lootName;
            if (Enum.TryParse(g.name, out lootName))
            {
                lootPrefabs.Add(lootName, g);
            }

        }
    }


    private void GenerateItemsForChunk(TerrainManager.TerrainChunk chunk)
    {
        float itemChance = chunk.itemChance;

        // Loop through each item position
        foreach (TerrainManager.TerrainChunk.Item item in chunk.items)
        {
            GameObject prefab;
            if (interactableItemWorldObjectPrefabs.TryGetValue(item.itemType, out prefab))
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




    public enum Loot
    {
        Coin
    }
}
