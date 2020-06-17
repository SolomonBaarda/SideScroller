using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResourceLoader : MonoBehaviour
{
    public UnityAction OnResourcesLoaded;

    public static ResourceLoader Instance;

    public GameObject SampleTerrainManagerPrefab;
    [HideInInspector]
    public SampleTerrainManager SampleTerrainManager;

    public bool ResourcesLoaded { get; private set; } = false;

    private void Awake()
    {
        Instance = this;
        ResourcesLoaded = false;

        OnResourcesLoaded += SceneLoader.EMPTY;
    }


    public void LoadAll()
    {
        if (!ResourcesLoaded)
        {
            DateTime before = DateTime.Now;
            LoadSampleTerrain();

            Debug.Log("Loaded all Game resources in " + (DateTime.Now - before).TotalSeconds + " seconds.");
            ResourcesLoaded = true;
            OnResourcesLoaded.Invoke();
        }
    }

    public IEnumerator WaitForLoadAll()
    {
        yield return null;
    }


    private void LoadSampleTerrain()
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
            SampleTerrainManager.LoadAllSampleTerrain();
        }


    }



}
