using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static UnityAction<Presets> OnSetPresets;
    public static UnityAction OnGameStart;
    public static UnityAction<Payload.Direction> OnGameEnd;

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

    public const float GAME_START_WAIT_SECONDS = 1f;

    // Game rules
    public Presets presets;

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
        isGameOver = true;

        // References to scripts
        playerManager = playerManagerObject.GetComponent<PlayerManager>();
        movingCamera = cameraGameObject.GetComponent<MovingCamera>();
        terrainManager = terrainManagerObject.GetComponent<TerrainManager>();
        chunkManager = chunkManagerObject.GetComponent<ChunkManager>();
        itemManager = itemManagerObject.GetComponent<ItemManager>();
        interactionManager = interactionManagerObject.GetComponent<InteractionManager>();
        enemyManager = enemyManagerObject.GetComponent<EnemyManager>();

        // Load HUD
        HUD.OnHUDLoaded += SetUpHUD;
        // Load presets
        OnSetPresets += Initialise;

        ItemManager.OnItemOutOfBounds += ItemOutOfBounds;

        OnGameEnd += EndGame;

        ChunkManager.OnChunkCreated += CheckStartOfGame;

        // If the Menu is loaded, wait for presets 
        if (SceneLoader.Instance != null && SceneLoader.Instance.SceneIsLoaded(SceneLoader.MENU_SCENE))
        {
        }
        // Just start the game and do defaut presets, must be running in the editor
        else
        {
            OnSetPresets.Invoke(new Presets());
        }


        // Call the UpdatePayload method repeatedly
        InvokeRepeating("UpdateChunks", 1, Chunk.UPDATE_CHUNK_REPEATING_DEFAULT_TIME);
    }

    private void OnDestroy()
    {
        HUD.OnHUDLoaded -= SetUpHUD;
        OnSetPresets -= Initialise;
        ItemManager.OnItemOutOfBounds -= ItemOutOfBounds;
        OnGameEnd -= EndGame;
        ChunkManager.OnChunkCreated -= CheckStartOfGame;
    }


    private void Initialise(Presets presets)
    {
        this.presets = presets;

        // Apply game rules
        if (presets.DoSinglePlayer)
        {
            presets.terrain_generation = TerrainManager.Generation.Multidirectional_Endless;
        }
        else
        {
            presets.terrain_generation = TerrainManager.Generation.Symmetrical_Limit;
        }

        terrainManager.LoadSampleTerrain(printDebug);

        // Generate spawn chunk
        terrainManager.GenerateSpawn(presets.terrain_generation, presets.terrain_limit_not_endless);
    }


    private void EndGame(Payload.Direction winningDirection)
    {
        if (!isGameOver)
        {
            isGameOver = true;

            // Get a list of all the players meant to be moving in that direction
            List<Player> winningPlayers = new List<Player>();
            foreach (Player p in playerManager.AllPlayers)
            {
                p.enabled = false;

                if (p.IdealDirection == winningDirection)
                {
                    winningPlayers.Add(p);
                }
            }
            // Get the message
            string winners = "";
            foreach (Player p in winningPlayers)
            {
                winners += p.PlayerID + " ";
            }

            Debug.Log(winners + "won!");
            Debug.Log("The game lasted " + GameTimeSeconds.ToString("0.0") + " seconds.");
        }
    }


    private void CheckStartOfGame(Vector2Int chunkID)
    {
        if(chunkID.Equals(ChunkManager.initialChunkID))
        {
            StartGame();
        }
    }




    private void StartGame()
    {
        Debug.Log("Game Starting!");

        if (presets.DoEnemySpawning)
        {
            enemyManager.ScanWholeNavMesh();
        }

        // Add only the spawn for initial start
        List<Chunk> onlySpawnChunk = new List<Chunk>
        {
            chunkManager.GetChunk(ChunkManager.initialChunkID)
        };

        // Single player game
        if (presets.DoSinglePlayer)
        {
            // Spawn player 1 
            playerManager.SpawnPlayer(Player.ID.P1, Payload.Direction.None, true, onlySpawnChunk, movingCamera.ViewBounds);

            // Set up camera
            movingCamera.SetFollowingTarget(playerManager.GetPlayer(Player.ID.P1).gameObject);
            movingCamera.direction = MovingCamera.Direction.Following;
        }
        // Multiplayer game
        else
        {
            // Spawn players 
            playerManager.SpawnPlayer(Player.ID.P1, Payload.Direction.Right, false, onlySpawnChunk, movingCamera.ViewBounds);
            playerManager.SpawnPlayer(Player.ID.P2, Payload.Direction.Left, true, onlySpawnChunk, movingCamera.ViewBounds);

            // Spawn payload
            Vector2 payloadSpawn = playerManager.GetBestRespawnPoint(Payload.Direction.None, onlySpawnChunk, movingCamera.ViewBounds);
            GameObject payload = itemManager.SpawnPayload(new Vector2(payloadSpawn.x, payloadSpawn.y + (terrainManager.CellSize.y)));

            // Set up camera
            movingCamera.SetFollowingTarget(payload);
            movingCamera.direction = MovingCamera.Direction.Following;

            if (hud != null)
            {
                hud.SetMultiplayer();
            }
        }

        foreach (Player p in playerManager.AllPlayers)
        {
            p.SetAlive();
        }

        isGameOver = false;
    }


    private void Update()
    {
        if (!isGameOver)
        {
            // Update the game time
            GameTimeSeconds += Time.deltaTime;

            // Check if a player is outside the bounds
            playerManager.CheckPlayersOutsideBounds(movingCamera.ViewBounds);
            // Check if a player needs to be respawned
            playerManager.CheckAllRespawns(chunkManager.LoadedChunks, movingCamera.ViewBounds);


            // Update HUD stuff
            if (hud != null)
            {
                Player player = playerManager.GetPlayer(Player.ID.P1);

                this.hud.SetVisible(true);
                HUD.HUDElements hud = new HUD.HUDElements(player.GetInventory<Coin>().Total, player.GetInventory<Health>().Total,
                    player.GetInventory<Health>().Max, GameTimeSeconds, fps_last_framerate);
                this.hud.UpdateHUD(in hud);
            }

            // Update the nav meshes
            if (presets.DoEnemySpawning)
            {
                // Check if we need to update the nav mesh
                foreach (Chunk c in chunkManager.LoadedChunks)
                {
                    UpdateNavMesh(c);
                }

                // Check if we need to update the size of the nav mesh
                Vector2Int currentMaxTilesFromOrigin = Vector2Int.zero;
                foreach (Chunk c in chunkManager.LoadedChunks)
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
    }



    private void UpdateChunks()
    {
        if (!isGameOver)
        {
            chunkManager.UpdateLoadedChunks(movingCamera.GetAllNearbyChunks());

            // Check each chunk
            CheckGenrateNewChunks(chunkManager.LoadedChunks);
        }

    }



    private void UpdateNavMesh(Chunk chunk)
    {
        Vector2 centre = chunk.transform.position;

        enemyManager.UpdateNavMesh(new Bounds(centre, chunk.bounds));
    }



    private bool CheckGenrateNewChunks(List<Chunk> toCheck)
    {
        bool chunksAreGenerating = false;

        // Check each chunk
        foreach (Chunk c in toCheck)
        {
            // Check if we need to generate any new chunks
            if(CheckGenerateNewChunks(c))
            {
                chunksAreGenerating = true;
            }
        }

        return chunksAreGenerating;
    }


    private bool CheckGenerateNewChunks(Chunk current)
    {
        bool chunksAreGenerating = false;

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
                // Get the SampleTerrain for the chunk in a symmetrical position
                int symmetricChunkIndex = -1;
                if (presets.terrain_generation.ToString().Contains("Symmetrical"))
                {
                    Vector2Int symmetricChunkID = exit.newChunkID;

                    // Try to get the symmetrical chunk. If it fails, just generate a random one
                    try
                    {
                        if (exit.newChunkID.x < ChunkManager.initialChunkID.x)
                        {
                            symmetricChunkID.x = ChunkManager.initialChunkID.x + Mathf.Abs(exit.newChunkID.x);
                        }
                        else if (exit.newChunkID.x > ChunkManager.initialChunkID.x)
                        {
                            symmetricChunkID.x = ChunkManager.initialChunkID.x - Mathf.Abs(exit.newChunkID.x);
                        }

                        Chunk symmetric = chunkManager.GetChunk(symmetricChunkID);
                        if (symmetric.chunkID != ChunkManager.initialChunkID)
                        {
                            symmetricChunkIndex = symmetric.sampleTerrainIndex;

                            // Generate the symmetrical chunk
                            terrainManager.Generate(exit.newChunkPositionWorld, exit.exitDirection, exit.newChunkID, terrainManager.GetSampleTerrain(symmetricChunkIndex), true);
                            chunksAreGenerating = true;
                            continue;
                        }
                    }
                    catch (Exception)
                    {
                        // Check that the symmetric chunk has not already been chosen and is being generated now 
                        if(terrainManager.ChunkAlreadyGenerating(symmetricChunkID))
                        {
                            chunksAreGenerating = true;
                            continue;
                        }
                    }
                }

                // Generate a new random chunk
                terrainManager.GenerateRandom(exit.newChunkPositionWorld, exit.exitDirection, exit.newChunkID, true);
                chunksAreGenerating = true;
                continue;
            }
        }

        return chunksAreGenerating;
    }


    private void ItemOutOfBounds(GameObject item)
    {
        if (!isGameOver)
        {
            if (WorldItem.ExtendsClass<Payload>(item))
            {
                Payload p = (Payload)WorldItem.GetClass<Payload>(item);

                // Get the best position on the screen
                try
                {
                    Vector2 respawn = playerManager.GetBestRespawnPoint(Payload.Direction.None, chunkManager.LoadedChunks, movingCamera.ViewBounds);
                    p.SetPosition(respawn);
                }
                catch (Exception)
                {
                    Debug.Log(item.name + " cannot be respawned. There are no respawn points nearby.");
                }
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
        public PresetValues player_gravity;
        public PresetValues player_speed;

        public Presets() : this(false, false, true, TerrainManager.Generation.Symmetrical_Limit, TerrainManager.DEAULT_WORLD_LENGTH_NOT_ENDLESS,
            PresetValues.Default, PresetValues.Default)
        {
        }

        public Presets(bool DoSinglePlayer, bool DoEnemySpawning, bool DoItemDrops,
            TerrainManager.Generation terrain_generation, int terrain_limit_not_endless,
            PresetValues player_gravity, PresetValues player_speed)
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


    public enum PresetValues
    {
        Default,
        Less,
        More,
        Random,
    }

    private void SetUpHUD(HUD hud)
    {
        this.hud = hud;

        if (isGameOver)
        {
            this.hud.SetVisible(false);
        }
    }
}
