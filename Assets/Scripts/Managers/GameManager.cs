﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static UnityAction<Presets> OnSetPresets;
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
        OnSetPresets += SetPresets;

        // If the Menu is loaded, wait for presets 
        if (SceneLoader.Instance != null && SceneLoader.Instance.SceneIsLoaded(SceneLoader.MENU_SCENE))
        {
            SceneLoader.Instance.OnScenesLoaded += StartGame;
        }
        // Just start the game and do defaut presets, must be running in the editor
        else
        {
            TerrainManager.OnInitialTerrainGenerated += StartGame;
            OnSetPresets.Invoke(new Presets());
        }
    }

    private void OnDestroy()
    {
        HUD.OnHUDLoaded -= SetUpHUD;
        OnSetPresets -= SetPresets;

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnScenesLoaded -= StartGame;
        }
        else
        {
            TerrainManager.OnInitialTerrainGenerated -= StartGame;
        }

        Menu.OnMenuClose -= StartGame;
    }


    private void StartGame()
    {
        if (presets.DoEnemySpawning)
        {
            enemyManager.ScanWholeNavMesh();
        }

        // Single player game
        if (presets.DoSinglePlayer)
        {
            // Spawn player 1 
            playerManager.SpawnPlayer(terrainManager.GetInitialTileWorldPositionForPlayer(), Player.ID.P1, Payload.Direction.None, true);

            // Set up camera
            movingCamera.SetFollowingTarget(playerManager.GetPlayer(Player.ID.P1).gameObject);
            movingCamera.direction = MovingCamera.Direction.Following;
        }
        // Multiplayer game
        else
        {
            // Spawn players 
            playerManager.SpawnPlayer(terrainManager.GetInitialTileWorldPositionForPlayer(), Player.ID.P1, Payload.Direction.Right, false);
            playerManager.SpawnPlayer(terrainManager.GetInitialTileWorldPositionForPlayer(), Player.ID.P2, Payload.Direction.Left, true);

            // Spawn payload
            GameObject payload = itemManager.SpawnPayload(new Vector2(0, 12));

            // Set up camera
            movingCamera.SetFollowingTarget(payload);
            movingCamera.direction = MovingCamera.Direction.Following;

            if(hud != null)
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


    private void SetPresets(Presets presets)
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

        // Generate terrain when the game loads
        terrainManager.Initialise(presets.terrain_generation, printDebug);

        playerManager.SetGameMode(presets.DoSinglePlayer);
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
            playerManager.CheckRespawns(movingCamera.GetAllNearbyChunks(), movingCamera.transform.position, movingCamera.ViewBounds);


            // Update HUD stuff
            if (hud != null)
            {
                Player player = playerManager.GetPlayer(Player.ID.P1);

                this.hud.SetVisible(true);
                HUD.HUDElements hud = new HUD.HUDElements(player.GetInventory<Coin>().Total, player.GetInventory<Health>().Total,
                    player.GetInventory<Health>().Max, GameTimeSeconds, fps_last_framerate);
                this.hud.UpdateHUD(in hud);
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



    private void LateUpdate()
    {
        if (!isGameOver)
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

    }



    private void UpdateNavMesh(Chunk chunk)
    {
        Vector2 centre = chunk.transform.position;

        enemyManager.UpdateNavMesh(new Bounds(centre, chunk.bounds));
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

                    // We have generated enough normal chunks
                    if (Mathf.Abs(chunk.x) >= presets.terrain_limit_not_endless ||
                        Mathf.Abs(chunk.y) >= presets.terrain_limit_not_endless)
                    {
                        // Need to generate the final chunk
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
        public PresetValues player_gravity;
        public PresetValues player_speed;

        public Presets() : this(false, false, true, TerrainManager.Generation.Symmetrical_Limit, TerrainManager.DEFAULT_MAX_CHUNKS_NOT_ENDLESS,
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
