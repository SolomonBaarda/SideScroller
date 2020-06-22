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


    public const string DefaultSeed = "";
    public string Seed { get; private set; }
    public int SeedHash => Seed.GetHashCode();

    public string RandomSeed { get { return Environment.TickCount.ToString(); } }
    private System.Random random;



    [Header("Camera")]
    public MovingCamera MovingCamera;

    [Header("Managers")]
    public TerrainManager TerrainManager;
    public ChunkManager ChunkManager;
    public PlayerManager PlayerManager;
    public ItemManager ItemManager;
    public InteractionManager InteractionManager;
    public AIManager AIManager;

    private HUD HUD;

    public GameStats Stats { get { return new GameStats(GameTimeSeconds, fps_last_framerate, PlayerManager); } }


    public float GameTimeSeconds { get; private set; }
    private bool isGameOver;

    public const float GAME_START_WAIT_SECONDS = 2f;

    // Game rules
    public Presets presets;
    public static Presets.VariableValue<float> GravityMultiplierPreset => new Presets.VariableValue<float>(1.0f, 0.5f, 1.5f, 0.25f, 2.0f);
    public static Vector2 DefaultGravity => new Vector2(0, -9.8f);


    // FPS variables
    [Header("FPS Settings")]
    private int fps_frame_counter = 0;
    private float fps_time_counter = 0.0f;
    private float fps_last_framerate = 0.0f;
    public float fps_refresh_time = 0.5f;
    public static int FPS_IDEAL = 60;

    [Header("Debug")]
    public bool ShowDebug = true;

    private void Awake()
    {
        isGameOver = true;

        // Load HUD
        HUD.OnHUDLoaded += SetUpHUD;
        // Load presets
        OnSetPresets += Initialise;

        ItemManager.OnItemOutOfBounds += ItemOutOfBounds;

        OnGameEnd += EndGame;

        ChunkManager.OnChunkCreated += CheckStartOfGame;

        // If the game scene is just open by its self, do default presets
        // This is most likely because it was run in the editor
        if (SceneLoader.Instance == null)
        {
            Presets p = new Presets
            {
                DoRandomSeed = false,
                DoEnemySpawning = true,
            };
            OnSetPresets.Invoke(p);
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
    }


    private void Initialise(Presets presets)
    {
        this.presets = presets;
        // Set the seed
        Seed = presets.Seed;
        if (presets.DoRandomSeed)
        {
            Seed = RandomSeed;
        }
        random = new System.Random(SeedHash);

        if (ShowDebug)
        {
            Debug.Log(presets.ToString());
            Debug.Log("Playing on seed: " + SeedHash);
        }

        // Set the gravity
        Physics2D.gravity = DefaultGravity;
        SetGravity(Presets.CalculateVariableValue(GravityMultiplierPreset, presets.GravityModifier, random));

        // Initialise ItemManager
        ItemManager.Initialise(presets.DoGenerateItemsWithWorld, presets.DoItemDrops, presets.DoSpawnWithRandomWeapons, ShowDebug, SeedHash);

        // Apply game rules
        int length = TerrainManager.WorldLengthPreset.Default;

        // Singleplayer
        if (!presets.DoMultiplayer)
        {
            presets.TerrainGenerationStyle = TerrainManager.Generation.Multidirectional_Endless;
        }
        // Multiplayer
        else
        {
            presets.TerrainGenerationStyle = TerrainManager.Generation.Symmetrical_Limit;
            length = Presets.CalculateVariableValue(TerrainManager.WorldLengthPreset, presets.TerrainWorldLengthIfNotEndless, random);
        }

        // Generate spawn chunk
        TerrainManager.GenerateSpawn(ResourceLoader.Instance.SampleTerrainManager, presets.TerrainGenerationStyle, length, SeedHash);
    }


    private void EndGame(Payload.Direction winningDirection)
    {
        if (!isGameOver)
        {
            isGameOver = true;

            // Get a list of all the players meant to be moving in that direction
            List<Player> winningPlayers = new List<Player>();
            foreach (Player p in PlayerManager.AllPlayers)
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

            if (ShowDebug)
            {
                Debug.Log(winners + "won!");
                Debug.Log("The game lasted " + GameTimeSeconds.ToString("0.0") + " seconds.");
            }
        }
    }


    private void CheckStartOfGame(Chunk c, TerrainManager.TerrainChunk t)
    {
        if (c.chunkID.Equals(ChunkManager.initialChunkID))
        {
            StartCoroutine(WaitForStartOfGame(GAME_START_WAIT_SECONDS));

            ChunkManager.OnChunkCreated -= CheckStartOfGame;
        }
    }



    private IEnumerator WaitForStartOfGame(float seconds)
    {
        int consecutiveFramesWithNoGeneration = 0;
        int targetFPS;

        do
        {
            if (TerrainManager.IsGenerating)
            {
                consecutiveFramesWithNoGeneration = 0;
            }
            else
            {
                consecutiveFramesWithNoGeneration++;
            }

            // Get the current number of frames in a second
            targetFPS = (int)fps_last_framerate;
            if (fps_last_framerate == 0)
            {
                targetFPS = FPS_IDEAL;
            }

            yield return null;

            // Wait for seconds * frames frames since no generation has taken place
        } while (consecutiveFramesWithNoGeneration <= seconds * targetFPS);

        // Then start the game
        StartGame();
    }



    private void StartGame()
    {
        // Add only the spawn for initial start
        List<Chunk> onlySpawnChunk = new List<Chunk> { ChunkManager.GetChunk(ChunkManager.initialChunkID) };

        // Calculate the player random speed
        float playerSpeed = Presets.CalculateVariableValue(PlayerMovement.SpeedPreset, presets.PlayerSpeed, random);

        // Single player game
        if (!presets.DoMultiplayer)
        {
            // Spawn player 1 
            Player p1 = PlayerManager.SpawnPlayer(Player.ID.P1, Payload.Direction.None, true, playerSpeed, onlySpawnChunk, MovingCamera.ViewBounds);

            // Set up camera
            MovingCamera.SetFollowingTarget(p1.gameObject);
            MovingCamera.direction = MovingCamera.Direction.Following;
            //AIManager.SetMeshUpdate(p1.transform);
        }
        // Multiplayer game
        else
        {
            // Spawn players 
            PlayerManager.SpawnPlayer(Player.ID.P1, Payload.Direction.Right, false, playerSpeed, onlySpawnChunk, MovingCamera.ViewBounds);
            PlayerManager.SpawnPlayer(Player.ID.P2, Payload.Direction.Left, true, playerSpeed, onlySpawnChunk, MovingCamera.ViewBounds);

            // Spawn payload
            Vector2 payloadSpawn = PlayerManager.GetBestRespawnPoint(Payload.Direction.None, onlySpawnChunk, MovingCamera.ViewBounds);
            GameObject payload = ItemManager.SpawnPayload(new Vector2(payloadSpawn.x, payloadSpawn.y + (TerrainManager.CellSize.y)));
            //payload.GetComponent<Payload>().SetFollowing(true, PlayerManager.GetPlayer(Player.ID.P1));

            // Set up camera
            MovingCamera.SetFollowingTarget(payload);
            MovingCamera.direction = MovingCamera.Direction.Following;

            // Set the graph to update
            if(presets.DoPathfinding)
            {
                AIManager.SetMeshUpdate(payload.transform);
            }
        }

        // Set all players to be alive
        foreach (Player p in PlayerManager.AllPlayers)
        {
            p.SetAlive();
        }

        if (HUD != null)
        {
            HUD.SetVisible(true);
        }

        isGameOver = false;

        if (ShowDebug)
        {
            Debug.Log("Game Starting!");
        }
    }


    public void SetGravity(float gravityMultiplier)
    {
        // Update the global gravity
        Vector2 newGrav = Physics2D.gravity * gravityMultiplier;
        if (newGrav.y > 0)
        {
            Physics2D.gravity = newGrav;
        }
    }


    private void Update()
    {
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

        // Input.GetButton("Scoreboard")
        UpdateHUD(Input.GetButton("Scoreboard"));

        if (!isGameOver)
        {
            // Update the game time
            GameTimeSeconds += Time.deltaTime;

            // Check if a player is outside the bounds
            PlayerManager.CheckPlayersOutsideBounds(MovingCamera.ViewBounds);
            // Check if a player needs to be respawned
            PlayerManager.CheckAllRespawns(ChunkManager.LoadedChunks, MovingCamera.ViewBounds);


            // Update the AIManager nav mesh
            if (presets.DoEnemySpawning)
            {
                // Scan the bounds of the nav mesh
                //AIManager.ScanBounds(Chunk.CalculateBounds(ChunkManager.LoadedChunks), TerrainManager.);
            }
        }
    }




    private void UpdateChunks()
    {
        ChunkManager.UpdateLoadedChunks(MovingCamera.GetAllNearbyChunks());

        // Check each chunk
        CheckGenrateNewChunks(ChunkManager.LoadedChunks);
    }



    private bool CheckGenrateNewChunks(List<Chunk> toCheck)
    {
        bool chunksAreGenerating = false;

        // Check each chunk
        foreach (Chunk c in toCheck)
        {
            // Check if we need to generate any new chunks
            if (CheckGenerateNewChunks(c))
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
                Chunk neighbour = ChunkManager.GetChunk(exit.newChunkID);
            }
            catch (Exception)
            {
                // Get the SampleTerrain for the chunk in a symmetrical position
                int symmetricChunkIndex = -1;
                if (presets.TerrainGenerationStyle.ToString().Contains("Symmetrical"))
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

                        Chunk symmetric = ChunkManager.GetChunk(symmetricChunkID);
                        if (symmetric.chunkID != ChunkManager.initialChunkID)
                        {
                            symmetricChunkIndex = symmetric.sampleTerrainIndex;

                            // Generate the symmetrical chunk
                            TerrainManager.Generate(exit.newChunkPositionWorld, exit.exitDirection, exit.newChunkID, TerrainManager.GetSampleTerrain(symmetricChunkIndex));
                            chunksAreGenerating = true;
                            continue;
                        }
                    }
                    catch (Exception)
                    {
                        // Check that the symmetric chunk has not already been chosen and is being generated now 
                        if (TerrainManager.ChunkAlreadyGenerating(symmetricChunkID))
                        {
                            chunksAreGenerating = true;
                            continue;
                        }
                    }
                }

                // Generate a new random chunk
                TerrainManager.GenerateRandom(exit.newChunkPositionWorld, exit.exitDirection, exit.newChunkID);
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
                    Vector2 respawn = PlayerManager.GetBestRespawnPoint(Payload.Direction.None, ChunkManager.LoadedChunks, MovingCamera.ViewBounds);
                    p.SetPosition(respawn);
                }
                catch (Exception)
                {
                    Debug.Log(item.name + " cannot be respawned. There are no respawn points nearby.");
                }
            }
        }
    }



    private void SetUpHUD(HUD hud)
    {
        HUD = hud;
        HUD.SetVisible(isGameOver);
    }




    public void UpdateHUD(bool showScoreboard)
    {
        // Display the scoreboard 
        if (HUD != null)
        {
            HUD.UpdateScoreboardStats(Stats);

            HUD.SetScoreboardVisible(showScoreboard);
        }
    }



    public class GameStats
    {
        public readonly float game_time_seconds;
        public readonly float last_fps;

        public readonly List<PlayerStats> Players = new List<PlayerStats>();

        public GameStats(float game_time_seconds, float last_fps, PlayerManager playerManager)
        {
            this.game_time_seconds = game_time_seconds;
            this.last_fps = last_fps;

            // Get the stats for each player
            foreach (Player p in playerManager.AllPlayers)
            {
                Players.Add(new PlayerStats(p));
            }
        }


        public class PlayerStats
        {
            public readonly string name;
            public readonly int deaths;
            public readonly int coins;

            public PlayerStats(Player player)
            {
                name = player.PlayerID.ToString();
                deaths = player.Deaths;
                coins = player.GetInventory<Coin>().Total;
            }
        }
    }

}
