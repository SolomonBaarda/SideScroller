using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemManager : MonoBehaviour
{
    public static UnityAction<Vector2> OnPotBroken;
    public static UnityAction<Vector2> OnChestOpened;

    public string seed;

    public GameObject[] lootTable;

    // Start is called before the first frame update
    void Start()
    {
        OnPotBroken += PotBroken;
        OnChestOpened += ChestOpened;
    }



    private void SpawnRandomLoot(Vector2 pos, GameObject[] loot)
    {
        System.Random r = new System.Random(seed.GetHashCode());

        int index = Mathf.FloorToInt(r.Next(0, loot.Length));

        GameObject g = Instantiate(loot[index]) as GameObject;
        g.transform.position = pos;
        g.transform.SetParent(transform);
    }


    private void PotBroken(Vector2 pos)
    {
        SpawnRandomLoot(pos, lootTable);
    }

    private void ChestOpened(Vector2 pos)
    {
        SpawnRandomLoot(pos, lootTable);
    }
}
