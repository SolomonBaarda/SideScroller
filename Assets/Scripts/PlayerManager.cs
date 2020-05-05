using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject playerPrefab;

    public List<Player> AllPlayers { get; private set; }

    private void Awake()
    {
        // Load all players
        AllPlayers = new List<Player>();
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
