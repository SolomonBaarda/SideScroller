using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerManager : MonoBehaviour
{
    public static UnityAction<Player> OnPlayerDie;

    public const float DEFAULT_RESPAWN_WAIT_TIME_SECONDS = 1f;
    private DateTime lastRespawnCheck;
    public const float RESPAWN_AREA_PERCENTAGE_OF_CAMERA_BOUNDS = 0.75f;

    public GameObject playerPrefab;

    public List<Player> AllPlayers { get; private set; } = new List<Player>();
    private Dictionary<Player, float> respawn = new Dictionary<Player, float>();

    private bool isSinglePlayer;

    private void Awake()
    {
        OnPlayerDie += SetPlayerDead;
    }

    private void OnDestroy()
    {
        OnPlayerDie -= SetPlayerDead;
    }

    public void SetGameMode(bool isSinglePlayer)
    {
        this.isSinglePlayer = isSinglePlayer;
    }

    private bool PlayerIsRespawning(Player p)
    {
        return respawn.ContainsKey(p);
    }

    private void SetPlayerDead(Player p)
    {
        p.SetDead();
        Debug.Log(p.PLAYER_LAYER + " has died.");

        if (!PlayerIsRespawning(p))
        {
            respawn.Add(p, DEFAULT_RESPAWN_WAIT_TIME_SECONDS);
        }
    }


    public void CheckPlayersOutsideBounds(Bounds screenBounds)
    {
        foreach (Player p in AllPlayers)
        {
            // Check if the player is within the screen bounds
            if (!PointIsWithinBounds(p.transform.position, screenBounds))
            {
                if (p.IsAlive)
                {
                    OnPlayerDie.Invoke(p);
                }
            }
        }
    }


    public void CheckRespawns(List<Chunk> chunksNearCamera, Vector2 cameraCentre, Bounds cameraViewBounds)
    {
        DateTime now = DateTime.Now;

        if (lastRespawnCheck == null)
        {
            lastRespawnCheck = now;
        }
        // Get the time since this method was last called
        TimeSpan time = now - lastRespawnCheck;
        lastRespawnCheck = now;

        // Check each player
        foreach (Player p in AllPlayers)
        {
            if (PlayerIsRespawning(p))
            {
                // Update the timer
                respawn[p] -= (float)(time.TotalMilliseconds / 1000);

                // Player needs to be respawned
                if (respawn[p] <= 0)
                {
                    Bounds respawnBounds = new Bounds(cameraViewBounds.center, cameraViewBounds.size * RESPAWN_AREA_PERCENTAGE_OF_CAMERA_BOUNDS);

                    // Try to respawn the player
                    if (WasRespawned(p, chunksNearCamera, cameraCentre, respawnBounds))
                    {
                        // Remove from the respawn list
                        respawn.Remove(p);
                    }
                    else
                    {
                        // Do nothing, will try to respawn again next time it is called
                    }
                }
            }
        }
    }


    private bool WasRespawned(Player player, List<Chunk> nearbyChunks, Vector2 cameraCentre, Bounds respawnBounds)
    {
        bool canRespawn = false;
        // Initialise to be the first respawn point
        Vector2 bestRespawnPosition = cameraCentre;

        if (isSinglePlayer)
        {
            // Do nothing for now, maybe just exit the game
        }
        else
        {
            // Loop through each chunk
            foreach (Chunk c in nearbyChunks)
            {
                // Each exit point 
                foreach (TerrainManager.TerrainChunk.Respawn point in c.respawnPoints)
                {
                    // Exit point is the correct direction
                    if (point.direction == Payload.Direction.None || player.IdealDirection == point.direction)
                    {
                        // Ensure the point is actually visible on the screen
                        if (PointIsWithinBounds(point.position, respawnBounds))
                        {
                            bool isNotClosestToAnotherPlayer = true;

                            // Ensure that the point is not on top of another player
                            if (isNotClosestToAnotherPlayer)
                            {
                                // Put the player on their side of the screen
                                // Choose the furthest away point from the payload
                                switch (player.IdealDirection)
                                {
                                    case Payload.Direction.Left:
                                        if (point.position.x > bestRespawnPosition.x)
                                        {
                                            bestRespawnPosition = point.position;
                                        }
                                        break;
                                    case Payload.Direction.Right:
                                        if (point.position.x < bestRespawnPosition.x)
                                        {
                                            bestRespawnPosition = point.position;
                                        }
                                        break;
                                    default:
                                        continue;
                                }

                                canRespawn = true;
                            }
                        }
                    }
                }
            }
        }

        // Respawn the player if there's a valid place
        if (canRespawn)
        {
            RespawnPlayer(player, bestRespawnPosition);
        }

        return canRespawn;
    }



    public static bool PointIsWithinBounds(Vector2 point, Bounds b)
    {
        return point.x >= b.min.x && point.x <= b.max.x && point.y >= b.min.y && point.y <= b.max.y;
    }

    private void RespawnPlayer(Player p, Vector2 position)
    {
        p.SetPosition(position);
        p.SetAlive();

        Debug.Log(p.PLAYER_LAYER + " has been respawned.");
    }


    public void SpawnPlayer(Vector2 position, Player.ID playerID, Payload.Direction directionToMove, bool canUseController)
    {
        GameObject g = Instantiate(playerPrefab, transform);
        g.name = playerID.ToString();

        Player p = g.GetComponent<Player>();
        p.SetPlayer(playerID, directionToMove, canUseController);
        AllPlayers.Add(p);

        p.SetPosition(position);
    }



    public Player GetPlayer(Player.ID ID)
    {
        // Check each player for ID
        foreach (Player p in AllPlayers)
        {
            if (p.PlayerID.Equals(ID))
            {
                return p;
            }
        }

        return null;
    }

}
