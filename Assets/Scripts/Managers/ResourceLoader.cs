using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResourceLoader : MonoBehaviour
{
    public static ResourceLoader Instance;

    [Header("Sample Terrain")]
    public GameObject SampleTerrainManagerPrefab;
    [HideInInspector]
    public SampleTerrainManager SampleTerrainManager { get; private set; }


    public Dictionary<string, GameObject> WorldItemPrefabs { get; private set; } = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> WeaponPrefabs { get; private set; } = new Dictionary<string, GameObject>();


    public bool ResourcesLoaded { get; private set; } = false;

    private void Awake()
    {
        Instance = this;
        ResourcesLoaded = false;
    }


    public void LoadAllThenCall(Action action)
    {
        // Load all resources
        StartCoroutine(WaitForLoadAllThenCall(action));
    }

    public IEnumerator WaitForLoadAllThenCall(Action action)
    {
        if (!ResourcesLoaded)
        {
            DateTime before = DateTime.Now;
            // Sample terrain
            yield return LoadSampleTerrain();

            // Item prefabs
            WorldItemPrefabs = LoadGameObjects("Prefabs/Items");
            yield return null;
            WeaponPrefabs = LoadGameObjects("Prefabs/Weapons");
            yield return null;

            Debug.Log("Loaded all Game resources in " + (DateTime.Now - before).TotalSeconds.ToString("0.00") + " seconds.");
            ResourcesLoaded = true;

            // Invoke the action
            action();
        }
    }


    private IEnumerator LoadSampleTerrain()
    {
        // Instantiate a new SampleTerrainManager
        if (GameObject.Find(SampleTerrainManager.Name) == null)
        {
            GameObject managerObject = Instantiate(SampleTerrainManagerPrefab, transform);
            managerObject.name = SampleTerrainManager.Name;

            SampleTerrainManager = managerObject.GetComponent<SampleTerrainManager>();
        }

        // Load the terrain
        if (!SampleTerrainManager.TerrainIsLoaded)
        {
            yield return SampleTerrainManager.LoadAllSampleTerrain();
        }
    }




    /// <summary>
    /// Loads all GameObjects from "Resources/path" and loads them into a Dictionary. The Key is the GameObject name.  
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Dictionary<string, GameObject> LoadGameObjects(string path)
    {
        // Load all objects from path
        GameObject[] allItemPrefabs = Resources.LoadAll<GameObject>(path);
        // Create dictionary
        Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

        // Loop through each object
        foreach (GameObject value in allItemPrefabs)
        {
            // Add it
            string key = value.name;
            prefabs.Add(key, value);
        }

        return prefabs;
    }


}
