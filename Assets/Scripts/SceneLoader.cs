using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    public static string GAME_SCENE = "Game";
    public static string MENU_SCENE = "Main Menu";
    public static string HUD_SCENE = "HUD";

    public GameObject loadingScreen;

    List<AsyncOperation> scenesLoading = new List<AsyncOperation>();

    private void Awake()
    {
        Instance = this;

        // Load the main menu
        loadingScreen.SetActive(true);
        scenesLoading.Add(SceneManager.LoadSceneAsync(MENU_SCENE, LoadSceneMode.Additive));
        StartCoroutine(GetSceneLoadProgress());
    }



    public void LoadGame()
    {
        loadingScreen.SetActive(true);
        scenesLoading.Add(SceneManager.LoadSceneAsync(GAME_SCENE, LoadSceneMode.Additive));
        scenesLoading.Add(SceneManager.LoadSceneAsync(HUD_SCENE, LoadSceneMode.Additive));

        scenesLoading.Add(SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(MENU_SCENE)));
        
        StartCoroutine(GetSceneLoadProgress());
    }



    private IEnumerator GetSceneLoadProgress()
    {
        for (int i = 0; i < scenesLoading.Count; i++)
        {
            while (!scenesLoading[i].isDone)
            {
                yield return null;
            }
        }

        // Hide the loading screen
        loadingScreen.SetActive(false);
    }




}
