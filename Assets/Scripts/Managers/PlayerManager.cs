using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerManager : MonoBehaviour
{
    public static UnityAction<Player> OnPlayerDie;
    public static UnityAction<Player> OnPlayerRespawn;

    public const float DEFAULT_RESPAWN_WAIT_TIME_SECONDS = 1f;
    private DateTime lastRespawnCheck;
    public const float RESPAWN_AREA_PERCENTAGE_OF_CAMERA_BOUNDS = 0.85f;

    public GameObject playerPrefab;

    public List<Player> AllPlayers { get; private set; } = new List<Player>();
    private Dictionary<Player, float> respawn = new Dictionary<Player, float>();


    private void Awake()
    {
        OnPlayerDie += SetPlayerDead;

        OnPlayerRespawn += SceneLoader.EMPTY;
    }

    private void OnDestroy()
    {
        OnPlayerDie -= SetPlayerDead;
        OnPlayerRespawn -= SceneLoader.EMPTY;
    }




    private bool PlayerIsRespawning(Player p)
    {
        return respawn.ContainsKey(p);
    }

    private void SetPlayerDead(Player p)
    {
        p.SetDead();

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


    public void CheckAllRespawns(List<Chunk> chunksNearCamera, Bounds cameraViewBounds)
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
                    // Try to respawn the player
                    if (WasRespawned(p, chunksNearCamera, cameraViewBounds))
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


    private bool WasRespawned(Player player, List<Chunk> nearbyChunks, Bounds cameraViewBounds)
    {
        try
        {
            // Get the best spawn point if we can
            Vector2 spawnPoint = GetBestRespawnPoint(player.IdealDirection, nearbyChunks, cameraViewBounds);

            // And respawn the player
            RespawnPlayer(player, spawnPoint);
            return true;
        }
        catch (Exception) { }

        return false;
    }



    public Vector2 GetBestRespawnPoint(Payload.Direction objectIdealDirection, List<Chunk> nearbyChunks, Bounds cameraViewBounds)
    {
        bool canRespawn = false;
        // Create new bounds, smaller then the camera screen used for respawns
        Bounds respawnBounds = new Bounds(cameraViewBounds.center, cameraViewBounds.size * RESPAWN_AREA_PERCENTAGE_OF_CAMERA_BOUNDS);
        // Initialise to be the first respawn point
        Vector2 bestRespawnPosition = respawnBounds.center;
        if (objectIdealDirection == Payload.Direction.None)
        {
            bestRespawnPosition = nearbyChunks[0].respawnPoints[0].position;
        }

        // Loop through each chunk
        foreach (Chunk c in nearbyChunks)
        {
            // Each exit point 
            foreach (TerrainManager.TerrainChunk.Respawn point in c.respawnPoints)
            {
                // Exit point is the correct direction
                if (point.direction == Payload.Direction.None || point.direction == objectIdealDirection)
                {
                    // Ensure the point is actually visible on the screen
                    if (PointIsWithinBounds(point.position, respawnBounds))
                    {
                        // Object on the correct side of the screen for the direction travveling
                        switch (objectIdealDirection)
                        {
                            // Ideal point is:
                            // Centre of screen
                            case Payload.Direction.None:
                                // New closest point to the centre
                                if (Vector2.Distance(point.position, respawnBounds.center) < Vector2.Distance(bestRespawnPosition, respawnBounds.center))
                                {
                                    bestRespawnPosition = point.position;
                                }
                                break;
                            // Right of screen
                            case Payload.Direction.Left:
                                if (point.position.x > bestRespawnPosition.x)
                                {
                                    bestRespawnPosition = point.position;
                                }
                                break;
                            // Left of screen
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

        // Return the best position if there is one 
        if (canRespawn)
        {
            return bestRespawnPosition;
        }
        else
        {
            throw new Exception("There is no valid respawn point on screen for direcion " + objectIdealDirection.ToString());
        }
    }




    public static bool PointIsWithinBounds(Vector2 point, Bounds b)
    {
        /*
        Debug.DrawLine(b.min, new Vector2(b.min.x, b.max.y), Color.black, 4);
        Debug.DrawLine(b.min, new Vector2(b.max.x, b.min.y), Color.black, 4);
        Debug.DrawLine(b.max, new Vector2(b.min.x, b.max.y), Color.black, 4);
        Debug.DrawLine(b.max, new Vector2(b.max.x, b.min.y), Color.black, 4);

        Debug.DrawRay(point, Vector3.forward, Color.red, 4);
        */

        return point.x >= b.min.x && point.x <= b.max.x && point.y >= b.min.y && point.y <= b.max.y;
    }



    private void RespawnPlayer(Player p, Vector2 position)
    {
        p.SetPosition(position);

        // Make the player face their ideal direction
        if(p.IdealDirection == Payload.Direction.Left)
        {
            p.Face(PlayerMovement.Direction.Left);
        }
        else
        {
            p.Face(PlayerMovement.Direction.Right);
        }

        p.SetAlive();

        // Call the event
        OnPlayerRespawn.Invoke(p);
    }




    public Player SpawnPlayer(Player.ID playerID, Payload.Direction directionToMove, bool canUseController, List<Chunk> nearbyChunks, Bounds cameraViewBounds)
    {
        try
        {
            // Try to spawn the player
            return SpawnPlayer(GetBestRespawnPoint(directionToMove, nearbyChunks, cameraViewBounds), playerID, directionToMove, canUseController);
        }
        catch (Exception)
        {
            throw new Exception("Player " + playerID + " could not be spawned.");
        }
    }


    private Player SpawnPlayer(Vector2 position, Player.ID playerID, Payload.Direction directionToMove, bool canUseController)
    {
        GameObject g = Instantiate(playerPrefab, transform);
        g.name = playerID.ToString();

        Player p = g.GetComponent<Player>();
        p.SetPlayer(playerID, directionToMove, canUseController);
        AllPlayers.Add(p);

        RespawnPlayer(p, position);

        return p;
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
