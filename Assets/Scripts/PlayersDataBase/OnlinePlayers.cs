using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Movement.Commands;
using UnityEngine.TextCore.Text;
using Movement.Components;

public class OnlinePlayers : NetworkBehaviour
{

    private Dictionary<ulong, GameObject> onlinePlayers = new();

    private void Start()
    {
        NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
    }

    public void AddPlayer(ulong id, GameObject player)
    {
        if (!IsServer) return;
        Debug.Log("SERVER: player añadido: " + id);
        onlinePlayers[id] = player;
        Debug.Log("SERVER: comprobacion de player: " + onlinePlayers[id].GetComponent<PlayerInformation>().Username);
    }

    //METODO PARA OBTENER UNA LISTA DE GAMEOBJECTS "PLAYER" A PARTIR DE UNA LISTA DE IDS
    public List<GameObject> GetPlayerListFromIds(List<ulong> id_players)
    {
        if (!IsServer) return null;
        List<GameObject> players = new List<GameObject>();

        foreach (var id in id_players)
        {
            players.Add(onlinePlayers[id]);
        }

        return players;
    }

    //METODO PARA OBTENER UNA LISTA DE GAMEOBJECTS "FIGHTER" A PARTIR DE UNA LISTA DE IDS
    public List<GameObject> GetFighterListFromIds(List<ulong> id_players)
    {
        if (!IsServer) return null;
        List<GameObject> fighters = new List<GameObject>();

        foreach (var id in id_players)
        {
            fighters.Add(onlinePlayers[id].GetComponent<PlayerInformation>().FighterObject);
        }

        return fighters;
    }

    //METODO PARA DEVOLVER UN GAMEOBJECT "PLAYER" ESPECIFICANDO SU ID
    public GameObject ReturnPlayerGameObject(ulong id)
    {
        if (!IsServer) return null;
        return onlinePlayers[id];
    }

    //METODO PARA DEVOLVER UN COMPONENTE "PLAYER INFORMATION" ESPECIFICANDO SU ID
    public PlayerInformation ReturnPlayerInformation(ulong id)
    {
        if (!IsServer) return null;
        return onlinePlayers[id].GetComponent<PlayerInformation>();
    }


    public void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        int currentLobbyId = onlinePlayers[clientId].GetComponent<PlayerInformation>().CurrentLobbyId;
        if (currentLobbyId == -1) return;

        Lobby lobby = GetComponent<LobbyManager>().GetLobbyFromId(currentLobbyId);

        GameObject fighter = null;
        if (lobby.IsStarted)
        {
            fighter = onlinePlayers[clientId].GetComponent<PlayerInformation>().FighterObject;
            if (fighter)
            {
                new TakeHitCommand(fighter.GetComponent<FighterMovement>(), 999).Execute();
                fighter.GetComponent<FighterInformation>().IsDisconnected = true;
            }
            else Debug.Log("PERSONAJE NO ENCONETRADO");
        }
        else lobby.RemovePlayerFromLobby(clientId);

    }


}