using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class OnlinePlayers
{

    Dictionary<ulong, GameObject> onlinePlayers;

    // Start is called before the first frame update
    public OnlinePlayers()
    {
        onlinePlayers = new Dictionary<ulong, GameObject>();
    }

    public void AddPlayer(ulong id, GameObject player)
    {
        onlinePlayers[id] = player;
    }

    public List<GameObject> GetPlayerListFromIds(List<ulong> id_players)
    {
        List<GameObject> players = new List<GameObject>();

        foreach (var id in id_players)
        {
            players.Add(onlinePlayers[id]);
        }

        return players;
    }

    public List<GameObject> GetFighterListFromIds(List<ulong> id_players)
    {
        List<GameObject> fighters = new List<GameObject>();

        foreach (var id in id_players)
        {
            fighters.Add(onlinePlayers[id].GetComponent<PlayerInformation>().FighterObject);
        }

        return fighters;
    }

    public GameObject ReturnPlayer(ulong id)
    {
        return onlinePlayers[id];
    }
}