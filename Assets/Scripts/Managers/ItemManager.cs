using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Linq;

public class ItemManager : MonoBehaviour
{
    public static string ITEM_LAYER = "Item";

    public static UnityAction<GameObject> OnGenerateLoot;
    public static UnityAction<GameObject> OnItemOutOfBounds;

    private System.Random random;

    private Dictionary<string, GameObject> worldObjectPrefabs;
    private Dictionary<string, GameObject> weaponPrefabs;

    public GameObject collectableItemPrefab;

    public GameObject payloadPrefab;
    public GameObject Payload { get; private set; }

    private void Awake()
    {
        DateTime before = DateTime.Now;

        worldObjectPrefabs = new Dictionary<string, GameObject>();
        weaponPrefabs = new Dictionary<string, GameObject>();

        // Load all items 
        LoadItemPrefabs(ref worldObjectPrefabs, "Prefabs/Items");
        LoadItemPrefabs(ref weaponPrefabs, "Prefabs/Weapons");

        DateTime after = DateTime.Now;
        TimeSpan time = after - before;

        Debug.Log("Loaded items in " + time.Milliseconds + "ms: (" + worldObjectPrefabs.Count + " world items), (" + weaponPrefabs.Count + " weapons)");

        TerrainManager.OnTerrainChunkGenerated += GenerateItemsForChunk;
        OnGenerateLoot += GenerateLootForItem;

        // Give the player a random weapon when they spawn
        PlayerManager.OnPlayerRespawn += GivePlayerRandomWeapon;

        random = new System.Random(0);
    }


    private void OnDestroy()
    {
        TerrainManager.OnTerrainChunkGenerated -= GenerateItemsForChunk;
        OnGenerateLoot -= GenerateLootForItem;

        PlayerManager.OnPlayerRespawn -= GivePlayerRandomWeapon;
    }


    public GameObject SpawnPayload(Vector2 position)
    {
        Payload = SpawnItem(payloadPrefab, position, "Payload");
        Payload.GetComponent<Payload>().SetPosition(position);
        Payload.transform.parent = null;
        return Payload;
    }

    private void GivePlayerRandomWeapon(Player p)
    {
        p.Inventory.DropRightHand();

        // Choose a random weapon from the loaded weapons
        GameObject prefab = weaponPrefabs.Values.ToArray()[random.Next(0, weaponPrefabs.Count)];

        // Spawn in that weapon
        GameObject weaponObject = SpawnItem(prefab, p.transform.position, prefab.name);
        // Make the player hold the weapon
        p.Inventory.PickUp(weaponObject);
    }


    private void GenerateLootForItem(GameObject item)
    {
        // Ensure its lootable
        if (WorldItem.ExtendsClass<ILootable>(item))
        {
            ILootable l = (ILootable)WorldItem.GetClass<ILootable>(item);
            LootTable table = l.GetLootTable();

            // Generate each item
            for (int i = 0; i < l.GetTotalItemsToBeLooted(); i++)
            {
                // Choose a random piece of loot
                int value = random.Next(0, table.GetTotalWeight());
                GameObject dropPrefab = table.GetLoot(value);

                // Spawn the drops
                SpawnItem(dropPrefab, item.transform.position, dropPrefab.name);
            }
        }
    }



    private void LoadAllScriptableItems<TEnum, TClass>(ref Dictionary<TEnum, TClass> prefabs, string path) where TEnum : struct where TClass : ScriptableObject
    {
        // Load all objects from path
        TClass[] allScripts = Resources.LoadAll<TClass>(path);

        // Loop through each object
        foreach (TClass t in allScripts)
        {
            // Try and get the enum type from the name
            if (!Enum.TryParse(t.name, out TEnum type))
            {
                Debug.LogError("Could not parse prefab " + t.name + " to enum type.");
                continue;
            }

            // Add it
            prefabs.Add(type, t);

            // TODO
            // GameObject is a loot item
            /*
            if (WorldItem.ImplementsInterface<ILoot>(t))
            {
                WorldItem loot = (WorldItem)WorldItem.GetScriptThatImplements<ILoot>(t);

                lootPrefabs.Add(loot.itemName, t);
            }
            */
        }
    }



    /// <summary>
    /// Load all prefabs from "path" and put them into a Dictionary
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="prefabs"></param>
    /// <param name="path"></param>
    private void LoadItemPrefabs(ref Dictionary<string, GameObject> prefabs, string path)
    {
        // Load all objects from path
        GameObject[] allItemPrefabs = Resources.LoadAll<GameObject>(path);

        // Loop through each object
        foreach (GameObject value in allItemPrefabs)
        {
            // Add it
            string key = value.name;
            prefabs.Add(key, value);
        }
    }


    private void GenerateItemsForChunk(TerrainManager.TerrainChunk chunk)
    {
        float itemChance = chunk.itemChance;

        // Loop through each item position
        foreach (TerrainManager.TerrainChunk.Item item in chunk.items)
        {
            // Check all the world items
            if (worldObjectPrefabs.TryGetValue(item.name, out GameObject prefab))
            {
                if (random.Next(0, 1) <= itemChance)
                {
                    SpawnItem(prefab, item.centreOfTile, item.name.ToString());
                }
            }
        }
    }


    private GameObject SpawnItem(GameObject item, Vector2 position, string name)
    {
        GameObject g = Instantiate(item, position, Quaternion.identity, transform);
        g.layer = LayerMask.NameToLayer(ITEM_LAYER);
        g.name = name;

        return g;
    }


    private void SpawnCollectableItem(ItemBase item, Vector2 position, string name, bool collideToPickUp, bool interactToPickUp)
    {
        GameObject g = SpawnItem(collectableItemPrefab, position, name);
        CollectableItem c = g.GetComponent<CollectableItem>();
        c.SetCollectableItem(item, collideToPickUp, interactToPickUp);
    }

}
