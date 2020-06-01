using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class ItemManager : MonoBehaviour
{
    public static string ITEM_LAYER = "Item";

    public static UnityAction<GameObject> OnGenerateLoot;
    public static UnityAction<GameObject> OnItemOutOfBounds;

    private System.Random random;

    private Dictionary<WorldItem.Name, GameObject> lootPrefabs;

    private Dictionary<WorldItem.Name, GameObject> worldObjectPrefabs;
    private Dictionary<Weapon.Name, Weapon> weaponScriptableObjectPrefabs;
    private Dictionary<Buff.Name, Buff> buffScriptableObjectPrefabs;

    public GameObject collectableItemPrefab;

    public GameObject payloadPrefab;
    public GameObject Payload { get; private set; }

    private void Awake()
    {
        DateTime before = DateTime.Now;

        lootPrefabs = new Dictionary<WorldItem.Name, GameObject>();
        worldObjectPrefabs = new Dictionary<WorldItem.Name, GameObject>();
        weaponScriptableObjectPrefabs = new Dictionary<Weapon.Name, Weapon>();
        buffScriptableObjectPrefabs = new Dictionary<Buff.Name, Buff>();

        // Load all items 
        LoadAllItemPrefabs(ref worldObjectPrefabs, "Prefabs/Items");
        LoadAllScriptableItems(ref weaponScriptableObjectPrefabs, "Scripts/Weapons");
        LoadAllScriptableItems(ref buffScriptableObjectPrefabs, "Scripts/Buffs");


        DateTime after = DateTime.Now;
        TimeSpan time = after - before;

        Debug.Log("Loaded items in " + time.Milliseconds + "ms: (" + worldObjectPrefabs.Count + " interactable items), (" + lootPrefabs.Count + " loot), (" + buffScriptableObjectPrefabs.Count + " buffs), (" +
                weaponScriptableObjectPrefabs.Count + " weapons)");

        TerrainManager.OnTerrainChunkGenerated += GenerateItemsForChunk;
        OnGenerateLoot += GenerateLootForItem;

        random = new System.Random(0);
    }


    private void OnDestroy()
    {
        TerrainManager.OnTerrainChunkGenerated -= GenerateItemsForChunk;
        OnGenerateLoot -= GenerateLootForItem;
    }


    public GameObject SpawnPayload(Vector2 position)
    {
        Payload = SpawnItem(payloadPrefab, position, "Payload");
        Payload.GetComponent<Payload>().SetPosition(position);
        return Payload;
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
                WorldItem.Name drop = table.GetLoot(value);

                GameObject g;
                if (!lootPrefabs.TryGetValue(drop, out g))
                {
                    Debug.LogError("Failed to get loot " + drop);
                    continue;
                }

                // Loot the item
                l.Loot();

                // Spawn the drops
                Vector2 pos = item.transform.position;
                SpawnItem(g, pos, drop.ToString());

                /*
                Buff b;
                buffScriptableObjectPrefabs.TryGetValue(Buff.Name.SpeedBoost, out b);
                SpawnCollectableItem(b, pos, b.buffName.ToString(), true, false) ;
                */
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
            TEnum type;
            if (!Enum.TryParse(t.name, out type))
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

            // GameObject is a loot item
            if (WorldItem.ExtendsClass<ILoot>(g))
            {
                WorldItem loot = (WorldItem)WorldItem.GetClass<ILoot>(g);

                lootPrefabs.Add(loot.itemName, g);
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
            if (worldObjectPrefabs.TryGetValue(item.itemType, out prefab))
            {
                if (random.Next(0, 1) <= itemChance)
                {
                    SpawnItem(prefab, item.centreOfTile, item.itemType.ToString());
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


    public enum Loot
    {
        Coin,
        Pot
    }
}
