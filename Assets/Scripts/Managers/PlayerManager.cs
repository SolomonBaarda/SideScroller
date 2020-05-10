using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerManager : MonoBehaviour
{
    public static UnityAction<Player> OnPlayerDie;

    public const float DEFAULT_RESPAWN_WAIT_TIME_SECONDS = 1f;
    private DateTime lastRespawnCheck;

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

    public void CheckRespawns(List<Chunk> chunksNearCamera, Payload payload)
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
                respawn[p] = respawn[p] - (float)time.TotalSeconds;

                // Player needs to be respawned
                if (respawn[p] <= 0)
                {
                    // Try to respawn the player
                    if (WasRespawned(p, chunksNearCamera, payload))
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


    private bool WasRespawned(Player player, List<Chunk> nearbyChunks, Payload payload)
    {
        bool canRespawn = false;
        Vector2 position = payload.gameObject.transform.position;

        if (isSinglePlayer)
        {
            // Do nothing for now, maybe just exit the game
        }
        else
        {
            // Loop through each chunk
            foreach(Chunk c in nearbyChunks)
            {
                // Each exit point 
                foreach (TerrainManager.TerrainChunk.Respawn point in c.respawnPoints)
                {
                    // Exit point is the correct direction
                    if(point.direction == player.IdealDirection || point.direction == Payload.Direction.None)
                    {
                        canRespawn = true;

                        // Put the player on their side of the screen
                        // Choose the furthest away point from the payload
                        if (player.IdealDirection == Payload.Direction.Left)
                        {
                            if(point.position.x > position.x)
                            {
                                position = point.position;
                            }
                        }
                        else if (player.IdealDirection == Payload.Direction.Right)
                        {
                            if (point.position.x < position.x)
                            {
                                position = point.position;
                            }
                        }
                    }
                }
            }

        }

        // Respawn the player if there's a valid place
        if (canRespawn)
        {
            RespawnPlayer(player, position);
        }

        return canRespawn;
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
