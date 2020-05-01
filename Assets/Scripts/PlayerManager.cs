using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public List<Player> AllPlayers { get; private set; }

    private void Awake()
    {
        // Load all players
        AllPlayers = new List<Player>(GetComponentsInChildren<Player>());
    }


    public Player GetPlayer(string ID)
    {
        // Check each player for ID
        foreach (Player p in AllPlayers)
        {
            if (p.PLAYER_ID.Equals(ID))
            {
                return p;
            }
        }

        return null;
    }

}
