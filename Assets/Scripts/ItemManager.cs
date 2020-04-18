using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemManager : MonoBehaviour
{
    public static UnityAction<Vector2> OnPotBroken;
    public static UnityAction<Vector2> OnChestOpened;

    private System.Random random;

    public GameObject[] lootTablePrefabs;

    private void Awake()
    {
        OnPotBroken += PotBroken;
        OnChestOpened += ChestOpened;

        random = new System.Random(0);
    }



    private void SpawnRandomLoot(Vector2 pos, GameObject[] loot)
    {
        int index = Mathf.FloorToInt(random.Next(0, loot.Length));

        GameObject g = Instantiate(loot[index]) as GameObject;
        g.transform.position = pos;
        g.transform.SetParent(transform);
    }


    private void PotBroken(Vector2 pos)
    {
        SpawnRandomLoot(pos, lootTablePrefabs);
    }

    private void ChestOpened(Vector2 pos)
    {
        SpawnRandomLoot(pos, lootTablePrefabs);
    }
}
