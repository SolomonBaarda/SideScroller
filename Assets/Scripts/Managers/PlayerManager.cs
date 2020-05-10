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

    public void CheckRespawns(List<Chunk> chunksNearCamera)
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
                    // Remove from the respawn list
                    respawn.Remove(p);

                    // Respawn the player
                    CheckRespawn(p, chunksNearCamera);
                }
            }
        }
    }


    private void CheckRespawn(Player p, List<Chunk> nearbyChunks)
    {
        Vector2 position = p.gameObject.transform.position;

        if(isSinglePlayer)
        {

        }
        else
        {
            
        }

        // Respawn the player
        RespawnPlayer(p, position);
    }


    private void RespawnPlayer(Player p, Vector2 position)
    {
        p.SetPosition(position);
        p.SetAlive();

        Debug.Log(p.PLAYER_LAYER + " has been respawned.");
    }


    public void SpawnPlayer(Vector2 position, Player.ID playerID, bool canUseController)
    {
        GameObject g = Instantiate(playerPrefab, transform);
        g.name = playerID.ToString();

        Player p = g.GetComponent<Player>();
        p.SetPlayer(playerID, canUseController);
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
