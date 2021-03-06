﻿using System.Collections;
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


    private const string StaticItemParentName = "Static Item Parent";
    public static Transform StaticItemParent
    {
        get
        {

            GameObject itemParent = GameObject.Find(StaticItemParentName);
            if (itemParent == null)
            {
                itemParent = new GameObject(StaticItemParentName);
            }

            return itemParent.transform;
        }
    }

    private System.Random random;

    private Dictionary<string, GameObject> worldObjectPrefabs;
    private Dictionary<string, GameObject> weaponPrefabs;

    public GameObject collectableItemPrefab;

    public GameObject payloadPrefab;
    public GameObject Payload { get; private set; }

    private bool spawnItemsWithWorldGeneration;
    private bool doItemDrops;
    private bool respawnPlayerWithRandomWeapon;

    public void Initialise(bool spawnItemsWithWorldGeneration, bool doItemDrops, bool respawnPlayerWithRandomWeapon, bool showDebug, int seedHash)
    {
        this.spawnItemsWithWorldGeneration = spawnItemsWithWorldGeneration;
        this.doItemDrops = doItemDrops;
        this.respawnPlayerWithRandomWeapon = respawnPlayerWithRandomWeapon;


        // Assign event calls
        ChunkManager.OnChunkCreated += GenerateItemsForChunk;
        OnGenerateLoot += GenerateLootForItem;
        // Give the player a random weapon when they spawn
        PlayerManager.OnPlayerRespawn += GivePlayerWeaponOnSpawn;

        // Assign the random
        random = new System.Random(seedHash);


        // Assign the prefab dictionaries
        if(ResourceLoader.Instance != null)
        {
            worldObjectPrefabs = ResourceLoader.Instance.WorldItemPrefabs;
            weaponPrefabs = ResourceLoader.Instance.WeaponPrefabs;
        }
    }


    private void OnDestroy()
    {
        // Remove all static event calls
        ChunkManager.OnChunkCreated -= GenerateItemsForChunk;
        OnGenerateLoot -= GenerateLootForItem;

        PlayerManager.OnPlayerRespawn -= GivePlayerWeaponOnSpawn;

        // Destroy the parent if it is here
        GameObject itemParent = GameObject.Find(StaticItemParentName);
        if (itemParent != null)
        {
            Destroy(StaticItemParent.gameObject);
        }
    }


    public GameObject SpawnPayload(Vector2 position)
    {
        Payload = SpawnItem(payloadPrefab, position, "Payload");
        Payload.GetComponent<Payload>().SetPosition(position);
        Payload.transform.parent = StaticItemParent;
        return Payload;
    }

    private void GivePlayerWeaponOnSpawn(Player p)
    {
        // If the player dies still with a weapon, destroy that weapon
        GameObject oldWeaponObject = p.Inventory.HeldItemRight;
        p.Inventory.DropRightHand();
        if (oldWeaponObject != null)
        {
            Destroy(oldWeaponObject);
        }


        GameObject prefab;
        // Give the player a random weapon
        if (respawnPlayerWithRandomWeapon)
        {
            // Choose a random weapon from the loaded weapons
            prefab = weaponPrefabs.Values.ToArray()[random.Next(0, weaponPrefabs.Count)];
        }
        // Give the player sword
        else
        {
            weaponPrefabs.TryGetValue("Shuriken", out prefab);
        }

        // Spawn in that weapon
        GameObject weaponObject = SpawnItem(prefab, p.transform.position, prefab.name);
        // Make the player hold the weapon
        p.Inventory.PickUp(weaponObject);
    }


    private void GenerateLootForItem(GameObject item)
    {
        if (doItemDrops)
        {
            // Ensure its lootable
            if (WorldItem.ExtendsClass<ILootable>(item))
            {
                ILootable l = (ILootable)WorldItem.GetClass<ILootable>(item);
                LootTable table = l.Table;

                // Generate each item
                for (int i = 0; i < l.TotalItemsToBeLooted; i++)
                {
                    // Choose a random piece of loot
                    int value = random.Next(0, table.GetTotalWeight());
                    GameObject dropPrefab = table.GetLoot(value);

                    // Spawn the drops
                    SpawnItem(dropPrefab, item.transform.position, dropPrefab.name);
                }
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






    private void GenerateItemsForChunk(Chunk chunk, TerrainManager.TerrainChunk terrainChunk)
    {
        if (spawnItemsWithWorldGeneration)
        {
            float itemChance = terrainChunk.itemChance;

            // Loop through each item position
            foreach (TerrainManager.TerrainChunk.Item item in terrainChunk.items)
            {
                // Check all the world items
                if (worldObjectPrefabs.TryGetValue(item.name, out GameObject prefab))
                {
                    if (random.Next(0, 1) <= itemChance)
                    {
                        GameObject g = SpawnItem(prefab, item.centreOfTile, item.name.ToString());
                        //g.transform.parent = chunk.ItemParent;
                    }
                }
            }
        }
    }


    private GameObject SpawnItem(GameObject itemPrefab, Vector2 position, string name)
    {
        GameObject g = Instantiate(itemPrefab, position, Quaternion.identity, transform);
        g.layer = LayerMask.NameToLayer(ITEM_LAYER);
        g.name = name;

        // Set the position precisely if it is a world item
        if (WorldItem.ExtendsClass<WorldItem>(g))
        {
            WorldItem i = (WorldItem)WorldItem.GetClass<WorldItem>(g);
            i.SetPosition(position);
        }

        return g;
    }


    private void SpawnCollectableItem(ItemBase item, Vector2 position, string name, bool collideToPickUp, bool interactToPickUp)
    {
        GameObject g = SpawnItem(collectableItemPrefab, position, name);
        CollectableItem c = g.GetComponent<CollectableItem>();
        c.SetCollectableItem(item, collideToPickUp, interactToPickUp);
    }

}
