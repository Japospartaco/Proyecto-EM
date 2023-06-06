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

    private void Start()
    {
        if (!IsServer) return;

    }

    //METODO PARA CREAR UNA SALA SABIENDO EL ID DEL CREADOR
    public void CreateLobby(ulong creatorId)
    {
        if (!IsServer)
        {
            Debug.Log("CLIENTE: no soy el server");
            return;
        }
        if (lobbies.Count >= MAX_LOBBIES) return;
        Debug.Log("SERVER: HASTA AQUI LLEGO");
        onlinePlayers = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<OnlinePlayers>();
        PlayerInformation creatorInformation = onlinePlayers.ReturnPlayerInformation(creatorId);
        Lobby lobby = new Lobby(creatorInformation, nextLobbyId, this);
        lobbies.Add(lobby);
        nextLobbyId++;
    }

    public void AddPlayerToLobby(int indexInList, ulong playerId)
    {
        if (!IsServer) return;
        lobbies[indexInList].AddPlayerToLobby(playerId);
    }

    //ELIMINAR LOBBY POR SU ID
    public void EliminateLobby(int idLobby)
    {
        if (!IsServer) return;
        lobbies.Remove(lobbies.Find(lobby => lobby.LobbyId == idLobby));
        LobbyEliminated?.Invoke(this, null);
    }
}
