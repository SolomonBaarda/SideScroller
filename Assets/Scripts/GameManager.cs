using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static UnityAction OnGameStart;

    [Header("Player")]
    public GameObject playerGameObject;
    private Player player;

    [Header("Camera")]
    public GameObject cameraGameObject;
    private MovingCamera movingCamera;

    [Header("Terrain Manager")]
    public GameObject terrainManagerObject;
    private TerrainManager terrainManager;

    [Header("Chunk Manager")]
    public GameObject chunkManagerObject;
    private ChunkManager chunkManager;

    [Header("Item Manager")]
    public GameObject itemManagerObject;
    private ItemManager itemManager;

    [Header("Interaction Manager")]
    public GameObject interactionManagerObject;
    private InteractionManager interactionManager;

    [Header("Enemy Manager")]
    public GameObject enemyManagerObject;
    private EnemyManager enemyManager;

    private HUD hud;

    [Header("Game Settings")]
    [SerializeField]
    private float gameTimeSeconds;
    private bool isGameOver;

    private int fps_frame_counter;
    private float fps_time_counter = 0.0f;
    private float fps_last_framerate = 0.0f;
    public float fps_refresh_time = 0.5f;


    private void Awake()
    {
        // References to scripts
        player = playerGameObject.GetComponent<Player>();
        movingCamera = cameraGameObject.GetComponent<MovingCamera>();
        terrainManager = terrainManagerObject.GetComponent<TerrainManager>();
        chunkManager = chunkManagerObject.GetComponent<ChunkManager>();
        itemManager = itemManagerObject.GetComponent<ItemManager>();
        interactionManager = interactionManagerObject.GetComponent<InteractionManager>();
        enemyManager = enemyManagerObject.GetComponent<EnemyManager>();

        // Add event calls 
        TerrainManager.OnTerrainGenerated += StartGame;
        //Menu.OnMenuClose += StartGame;

        isGameOver = true;

        HUD.OnHUDLoaded += SetUpHUD;
        SceneManager.LoadSceneAsync("HUD", LoadSceneMode.Additive);
    }


    private void Start()
    {
        // Generate terrain when the game loads
        terrainManager.Initialise();
    }

    private void OnDestroy()
    {
        Menu.OnMenuClose -= StartGame;
    }


    private void Update()
    {
        // Update the game time
        if (!isGameOver)
        {
            gameTimeSeconds += Time.deltaTime;
        }

        // Update FPS
        if (fps_time_counter < fps_refresh_time)
        {
            fps_time_counter += Time.deltaTime;
            fps_frame_counter++;
        }
        else
        {
            fps_last_framerate = fps_frame_counter / fps_time_counter;
            fps_frame_counter = 0;
            fps_time_counter = 0;
        }

        // Quit the build
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        // Toggle camera mode
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            switch (movingCamera.direction)
            {
                case MovingCamera.Direction.Terrain:
                    movingCamera.direction = MovingCamera.Direction.Following;
                    break;
                case MovingCamera.Direction.Stationary:
                    movingCamera.direction = MovingCamera.Direction.Terrain;
                    break;
                case MovingCamera.Direction.Following:
                    movingCamera.direction = MovingCamera.Direction.Stationary;
                    break;
            }
        }

        if (Input.GetKey(KeyCode.R))
        {
            player.SetAlive();
        }

        // Update HUD stuff
        HUD.HUDElements hud = new HUD.HUDElements(player.GetInventory<Coin>().Total, player.GetInventory<Health>().Total,
            player.GetInventory<Health>().Max, gameTimeSeconds, fps_last_framerate);
        this.hud.UpdateHUD(in hud);
    }



    private void LateUpdate()
    {
        // Check each chunk
        List<Chunk> nearbyChunksToCamera = movingCamera.GetAllNearbyChunks();
        foreach (Chunk c in nearbyChunksToCamera)
        {
            // Generate any new chunks if necessary
            CheckGenerateNewChunks(c);

            // Update the nav meshes
            UpdateNavMesh(c);
        }
    }



    private void UpdateNavMesh(Chunk chunk)
    {
        Vector2 centre = chunk.transform.position;

        enemyManager.UpdateNavMesh(new Bounds(centre, chunk.bounds));
    }


    private void StartGame()
    {
        enemyManager.ScanWholeNavMesh();

        player.SetPosition(terrainManager.GetInitialTileWorldPositionForPlayer());
        player.SetAlive();

        movingCamera.direction = MovingCamera.Direction.Following;

        isGameOver = false;
    }



    private void CheckGenerateNewChunks(Chunk current)
    {
        // See if any of the neighbour chunks exists
        foreach (TerrainManager.TerrainChunk.Exit exit in current.exits)
        {
            try
            {
                // Chunk already exists, do nothing
                Chunk neighbour = chunkManager.GetChunk(exit.newChunkID);

                // Check if the paths need to be updated
                foreach(CameraPath p in neighbour.cameraPaths)
                {
                    if(!p.hasExtraPointStart)
                    {
                        Vector2[] currentPathPoints = current.cameraPaths.Find(x => x.nextChunk.Equals(neighbour.chunkID)).points;

                        // Add the second last point from the current path
                        p.AddExtraPointAtStart(currentPathPoints[currentPathPoints.Length - 2]);
                    }
                    if(!p.hasExtraPointEnd)
                    {
                        try
                        {
                            Chunk next = chunkManager.GetChunk(p.nextChunk);
                            Vector2[] nextPathPoints = next.cameraPaths.ToArray()[0].points;

                            // Add the second point from the next path
                            p.AddExtraPointAtEnd(nextPathPoints[1]);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Does not exist, so generate it
                // Calculate how far along the camera path the new chunk is
                float distanceFromOrigin = current.CalculateNewChunkDistanceFromOrigin(MovingCamera.GetClosestCameraPath(exit.exitPositionWorld, current));
                // Generate the new chunk
                terrainManager.Generate(exit.newChunkPositionWorld, exit.exitDirection, distanceFromOrigin, exit.newChunkID);
            }

            // Check if we need to update the size of the nav mesh
            enemyManager.CheckUpdateSize(new Vector2Int(Mathf.Abs(exit.tilesFromOrigin.x), Mathf.Abs(exit.tilesFromOrigin.y)));
        }
    }




    private void SetUpHUD(HUD hud)
    {
        this.hud = hud;
    }
}
