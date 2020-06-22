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

    private List<AsyncOperation> scenesLoading = new List<AsyncOperation>();
    public UnityAction OnScenesLoaded;

    private Presets lastPreset;

    public bool IsLoadingScenes { get { return scenesLoading.Count > 0; } }

    private void Awake()
    {
        Instance = this;

        SceneManager.sceneLoaded += OnSceneLoaded;

        // Add an empty method to the event call to ensure never null
        OnScenesLoaded += EMPTY;

        // Load the main menu
        loadingScreen.SetActive(true);
        scenesLoading.Add(SceneManager.LoadSceneAsync(MENU_SCENE, LoadSceneMode.Additive));
        StartCoroutine(WaitForLoadScenes());
    }




    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        OnScenesLoaded -= EMPTY;
    }



    private void Update()
    {
        // Quit the build
        if (Input.GetButtonDown("Cancel"))
        {
            
        }
    }


    public void Quit()
    {
        Application.Quit();
    }

    public void LoadGame(Presets presets)
    {
        // Update the presets
        lastPreset = presets;

        // Load the game and HUD
        loadingScreen.SetActive(true);
        scenesLoading.Add(SceneManager.LoadSceneAsync(GAME_SCENE, LoadSceneMode.Additive));
        scenesLoading.Add(SceneManager.LoadSceneAsync(HUD_SCENE, LoadSceneMode.Additive));

        // Unload the menu
        scenesLoading.Add(SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(MENU_SCENE)));

        StartCoroutine(WaitForLoadScenes());
    }


    public void UnloadGameToMenu()
    {
        loadingScreen.SetActive(true);

        // Load the menu
        scenesLoading.Add(SceneManager.LoadSceneAsync(MENU_SCENE, LoadSceneMode.Additive));

        // Unload the game
        scenesLoading.Add(SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(GAME_SCENE)));
        scenesLoading.Add(SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(HUD_SCENE)));

        StartCoroutine(WaitForLoadScenes());
    }


    private IEnumerator WaitForLoadScenes()
    {
        // Loop through each scene that is loading
        for (int i = 0; i < scenesLoading.Count; i++)
        {
            // Return null while it is still loading
            while (!scenesLoading[i].isDone)
            {
                yield return null;
            }
        }

        // Hide the loading screen
        loadingScreen.SetActive(false);

        // Event call
        OnScenesLoaded.Invoke();
    }


    private void OnSceneLoaded(Scene s, LoadSceneMode l)
    {
        if (s.name.Equals(GAME_SCENE))
        {
            GameManager.OnSetPresets.Invoke(lastPreset);
        }
        if (s.name.Equals(MENU_SCENE))
        {
            Menu m;
            foreach(GameObject g in s.GetRootGameObjects())
            {
                m = g.GetComponent<Menu>();
                if (m != null)
                {
                    ResourceLoader.Instance.LoadAllThenCall(m.EnablePlayButton);
                    return;
                }
            }

            Debug.LogError("Menu script could not be found in scene.");
        }
    }


    public bool SceneIsLoaded(string name)
    {
        return SceneManager.GetSceneByName(name) != null;
    }


    public static void EMPTY() { }

    public static void EMPTY<T>(T _) { }

    public static void EMPTY<T, U>(T _, U __) { }
}
