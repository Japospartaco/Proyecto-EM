using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] private List<Lobby> lobbies = new(); //lista de lobbies creados

    private OnlinePlayers onlinePlayers;

    private int nextLobbyId = 0;    //variable encargada de asignar id a las lobbies

    private const int MAX_LOBBIES = 4;  //numeor maximo de lobbies, ahora mismo fijado a 4 por comodidad

    public EventHandler LobbyEliminated; //evento en caso de eliminar lobbies, no se usa

    public List<Lobby> Lobbies { get { return lobbies; } }  //metodo de acceso a la lista de lobbies


    //METODO PARA CREAR UNA SALA SABIENDO EL ID DEL CREADOR, devuelve false si no es capaz
    public bool CreateLobby(ulong creatorId, bool isPrivate, string password)
    {
        if (!IsServer)
        {
            return false;
        }

        //si se alcanza el maximo de lobbies returnea
        if (lobbies.Count >= MAX_LOBBIES) return false;

        onlinePlayers = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<OnlinePlayers>();
        PlayerInformation creatorInformation = onlinePlayers.ReturnPlayerInformation(creatorId);

        Lobby lobby;
        //creamos lobby privado o publico segun se especifique
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

    //metodo para añadir personas a lobbies, devuelve false si no ew capaz
    public bool AddPlayerToLobby(int indexInList, ulong playerId)
    {
        if (!IsServer) return false;
        return lobbies[indexInList].AddPlayerToLobby(playerId);

    }

    //ELIMINAR LOBBY POR SU ID ---> NUNCA SE LLEGA A USAR
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


    //metodo que devuelve el lobby en el que se encunetra el jugador especificado
    public int GetPlayersLobby(ulong playerId)
    {
        if (!IsServer) return -1;

        return onlinePlayers.ReturnPlayerInformation(playerId).CurrentLobbyId;
    }


    //metodo que devuelve el lobby con la id especificada
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
