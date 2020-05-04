using Pathfinding;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static UnityAction OnGameStart;

    [Header("Player")]
    public GameObject playerManagerObject;
    private PlayerManager playerManager;

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

    public float GameTimeSeconds { get; private set; }
    private bool isGameOver;

    // Game rules
    public Presets presets = new Presets();

    // FPS variables
    [Header("FPS Settings")]
    private int fps_frame_counter;
    private float fps_time_counter = 0.0f;
    private float fps_last_framerate = 0.0f;
    public float fps_refresh_time = 0.5f;

    [Header("Debug")]
    public bool printDebug = true;

    private void Awake()
    {
        // References to scripts
        playerManager = playerManagerObject.GetComponent<PlayerManager>();
        movingCamera = cameraGameObject.GetComponent<MovingCamera>();
        terrainManager = terrainManagerObject.GetComponent<TerrainManager>();
        chunkManager = chunkManagerObject.GetComponent<ChunkManager>();
        itemManager = itemManagerObject.GetComponent<ItemManager>();
        interactionManager = interactionManagerObject.GetComponent<InteractionManager>();
        enemyManager = enemyManagerObject.GetComponent<EnemyManager>();

        // Add event calls 
        TerrainManager.OnInitialTerrainGenerated += StartGame;
        //Menu.OnMenuClose += StartGame;

        isGameOver = true;

        HUD.OnHUDLoaded += SetUpHUD;
        SceneManager.LoadSceneAsync("HUD", LoadSceneMode.Additive);
    }


    private void Start()
    {
        // Apply game rules
        if (presets.DoSinglePlayer)
        {
            presets.terrain_generation = TerrainManager.Generation.Multidirectional_Endless;
        }

        // Generate terrain when the game loads
        terrainManager.Initialise(presets.terrain_generation, printDebug);
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
            GameTimeSeconds += Time.deltaTime;
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
        if (Input.GetButton("Cancel"))
        {
            Application.Quit();
        }



        if (Input.GetKey(KeyCode.R))
        {
            foreach (Player p in playerManager.AllPlayers)
            {
                p.SetAlive();
            }

        }


        Player player = playerManager.GetPlayer(Player.ID.P1);

        // Update HUD stuff
        HUD.HUDElements hud = new HUD.HUDElements(player.GetInventory<Coin>().Total, player.GetInventory<Health>().Total,
            player.GetInventory<Health>().Max, GameTimeSeconds, fps_last_framerate);
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
            if (presets.DoEnemySpawning)
            {
                UpdateNavMesh(c);
            }
        }

        // Check if we need to update the size of the nav mesh
        if (presets.DoEnemySpawning)
        {
            // Check if we need to update the nav mesh
            Vector2Int currentMaxTilesFromOrigin = Vector2Int.zero;
            foreach (Chunk c in nearbyChunksToCamera)
            {
                foreach (TerrainManager.TerrainChunk.Exit e in c.exits)
                {
                    int x = Mathf.Abs(e.tilesFromOrigin.x);
                    int y = Mathf.Abs(e.tilesFromOrigin.y);

                    if (x > currentMaxTilesFromOrigin.x)
                    {
                        currentMaxTilesFromOrigin.x = x;
                    }
                    if (y > currentMaxTilesFromOrigin.y)
                    {
                        currentMaxTilesFromOrigin.y = y;
                    }
                }
            }

            enemyManager.CheckUpdateSize(currentMaxTilesFromOrigin);
        }
    }



    private void UpdateNavMesh(Chunk chunk)
    {
        Vector2 centre = chunk.transform.position;

        enemyManager.UpdateNavMesh(new Bounds(centre, chunk.bounds));
    }


    private void StartGame()
    {
        if (presets.DoEnemySpawning)
        {
            enemyManager.ScanWholeNavMesh();
        }

        // Spawn players
        playerManager.SpawnPlayer(terrainManager.GetInitialTileWorldPositionForPlayer(), Player.ID.P1);
        if (!presets.DoSinglePlayer)
        {
            playerManager.SpawnPlayer(terrainManager.GetInitialTileWorldPositionForPlayer(), Player.ID.P2);
        }

        foreach (Player p in playerManager.AllPlayers)
        {
            p.SetAlive();
        }

        GameObject payload = itemManager.SpawnPayload(new Vector2(0, 12));

        movingCamera.SetFollowingTarget(payload);
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
            }
            catch (Exception)
            {
                // Don't generate the chunk if there is a limit
                if (presets.terrain_generation.ToString().Contains("Limit"))
                {
                    Vector2Int chunk = exit.newChunkID - ChunkManager.initialChunkID;

                    if (Mathf.Abs(chunk.x) >= presets.terrain_limit_not_endless ||
                        Mathf.Abs(chunk.y) >= presets.terrain_limit_not_endless)
                    {
                        continue;
                    }
                }

                // Get the SampleTerrain for the chunk in a symmetrical position
                int symmetricChunkIndex = -1;
                if (presets.terrain_generation.ToString().Contains("Symmetrical"))
                {
                    try
                    {
                        Vector2Int symmetricChunkID = exit.newChunkID;
                        if(exit.newChunkID.x < ChunkManager.initialChunkID.x)
                        {
                            symmetricChunkID.x = ChunkManager.initialChunkID.x + Mathf.Abs(exit.newChunkID.x);
                        }
                        else if(exit.newChunkID.x > ChunkManager.initialChunkID.x)
                        {
                            symmetricChunkID.x = ChunkManager.initialChunkID.x - Mathf.Abs(exit.newChunkID.x);
                        }

                        Chunk symmetric = chunkManager.GetChunk(symmetricChunkID);
                        if (symmetric.chunkID != ChunkManager.initialChunkID)
                        {
                            symmetricChunkIndex = symmetric.sampleTerrainIndex;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                // Generate the new chunk
                terrainManager.Generate(exit.newChunkPositionWorld, exit.exitDirection, exit.newChunkID, symmetricChunkIndex);
            }
        }
    }



    public class Presets
    {
        // Game rules
        public bool DoSinglePlayer;
        public bool DoEnemySpawning;
        public bool DoItemDrops;

        // Map generation stuff
        public TerrainManager.Generation terrain_generation;
        public int terrain_limit_not_endless;

        // Objective stuff

        // Player controller stuff
        public Preset player_gravity;
        public Preset player_speed;

        public Presets() : this(false, false, true, TerrainManager.Generation.Symmetrical_Limit, TerrainManager.DEFAULT_MAX_CHUNKS_NOT_ENDLESS, 
            Preset.Default, Preset.Default)
        {
        }

        public Presets(bool DoSinglePlayer, bool DoEnemySpawning, bool DoItemDrops,
            TerrainManager.Generation terrain_generation, int terrain_limit_not_endless,
            Preset player_gravity, Preset player_speed)
        {
            this.DoSinglePlayer = DoSinglePlayer;
            this.DoEnemySpawning = DoEnemySpawning;
            this.DoItemDrops = DoItemDrops;

            this.terrain_generation = terrain_generation;
            this.terrain_limit_not_endless = terrain_limit_not_endless;



            this.player_gravity = player_gravity;
            this.player_speed = player_speed;
        }
    }


    public enum Preset
    {
        Default,
        Less,
        More,
        Random,
    }

    private void SetUpHUD(HUD hud)
    {
        this.hud = hud;
    }
}
