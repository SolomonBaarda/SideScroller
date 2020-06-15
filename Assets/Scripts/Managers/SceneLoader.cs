using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    public static string GAME_SCENE = "Game";
    public static string MENU_SCENE = "Main Menu";
    public static string HUD_SCENE = "HUD";

    public GameObject loadingScreen;

    List<AsyncOperation> scenesLoading = new List<AsyncOperation>();
    public UnityAction OnScenesLoaded;

    private Presets lastPreset;

    public bool IsLoadingScenes { get { return scenesLoading.Count > 0; } }

    private void Awake()
    {
        Instance = this;

        // Add an empty method to the event call to ensure never null
        OnScenesLoaded += EMPTY;

        // Load the main menu
        loadingScreen.SetActive(true);
        scenesLoading.Add(SceneManager.LoadSceneAsync(MENU_SCENE, LoadSceneMode.Additive));
        StartCoroutine(WaitForLoadScenes());
    }


    private void Update()
    {
        // Quit the build
        if (Input.GetButton("Cancel"))
        {
            Application.Quit();
        }
    }


    public void LoadGame(Presets presets)
    {
        lastPreset = presets;

        // Load the game and HUD
        loadingScreen.SetActive(true);
        scenesLoading.Add(SceneManager.LoadSceneAsync(GAME_SCENE, LoadSceneMode.Additive));
        scenesLoading.Add(SceneManager.LoadSceneAsync(HUD_SCENE, LoadSceneMode.Additive));

        // Unload the menu
        scenesLoading.Add(SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(MENU_SCENE)));

        SceneManager.sceneLoaded += OnSceneLoaded;

        StartCoroutine(WaitForLoadScenes());
    }


    public void UnloadGameToMenu()
    {
        // Load the menu
        loadingScreen.SetActive(true);
        if (SceneIsLoaded(MENU_SCENE))
        {
            scenesLoading.Add(SceneManager.LoadSceneAsync(MENU_SCENE, LoadSceneMode.Additive));
        }

        // Unload the game
        if (SceneIsLoaded(GAME_SCENE))
        {
            scenesLoading.Add(SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(GAME_SCENE)));
        }
        if (SceneIsLoaded(HUD_SCENE))
        {
            scenesLoading.Add(SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(HUD_SCENE)));
        }

        StartCoroutine(WaitForLoadScenes());
    }


    private IEnumerator WaitForLoadScenes()
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

        OnScenesLoaded.Invoke();
    }


    private void OnSceneLoaded(Scene s, LoadSceneMode l)
    {
        if (s.name.Equals(GAME_SCENE))
        {
            GameManager.OnSetPresets.Invoke(lastPreset);
        }
    }


    public bool SceneIsLoaded(string name)
    {
        return SceneManager.GetSceneByName(name) != null;
    }


    public static void EMPTY() { }

    public static void EMPTY<T>(T _) { }
}
