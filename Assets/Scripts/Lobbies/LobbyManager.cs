using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] private List<Lobby> lobbies = new();

    private OnlinePlayers onlinePlayers;

    private int nextLobbyId = 0;

    private const int MAX_LOBBIES = 4;

    public EventHandler LobbyEliminated;

    public List<Lobby> Lobbies { get { return lobbies; } }


    //METODO PARA CREAR UNA SALA SABIENDO EL ID DEL CREADOR
    public bool CreateLobby(ulong creatorId, bool isPrivate, string password)
    {
        if (!IsServer)
        {
            Debug.Log("CLIENTE: no soy el server");
            return false;
        }

        if (lobbies.Count >= MAX_LOBBIES) return false;


        Debug.Log("SERVER: HASTA AQUI LLEGO");
        onlinePlayers = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<OnlinePlayers>();
        PlayerInformation creatorInformation = onlinePlayers.ReturnPlayerInformation(creatorId);

        Lobby lobby;
        if (isPrivate)
        {
            lobby = new Lobby(creatorInformation, nextLobbyId, this, password);
        }
        else
        {
            lobby = new Lobby(creatorInformation, nextLobbyId, this);
        }

        lobbies.Add(lobby);
        nextLobbyId++;
        return true;
    }

    public bool AddPlayerToLobby(int indexInList, ulong playerId)
    {
        if (!IsServer) return false;
        return lobbies[indexInList].AddPlayerToLobby(playerId);

    }

    //ELIMINAR LOBBY POR SU ID
    /*
    public void EliminateLobby(int idLobby)
    { 
        if (!IsServer) return;
        Lobby lobby = lobbies.Find(lobby => lobby.LobbyId == idLobby);

        lobbies.Remove(lobby);

        SuscribirLobby(lobby);

        LobbyEliminated?.Invoke(this, null);
    }

    public void SuscribirLobby(Lobby lobby)
    {
        lobby.SuscribirEliminar();
    }*/

    public int GetPlayersLobby(ulong playerId)
    {
        if (!IsServer) return -1;

        return onlinePlayers.ReturnPlayerInformation(playerId).CurrentLobbyId;
    }

    public Lobby GetLobbyFromId(int lobbyId)
    {
        foreach (var lob in lobbies)
        {
            if (lob.LobbyId == lobbyId)
                return lob;
        }
        return null;
    }
}
