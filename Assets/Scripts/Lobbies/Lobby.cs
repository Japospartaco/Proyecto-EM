using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditor;
using UnityEngine;

public class Lobby
{
    private int lobbyId = -1;
    private bool isPrivate = false;
    private bool isStarted = false;
    private int roundNumber = 1;
    private int roundTime = 5;


    private List<PlayerInformation> playersInformation = new();
    private const int MAX_PLAYERS = 4;
    private List<ulong> readyPlayers = new();

    private OnlinePlayers onlinePlayers;
    private LobbyManager lobbyManager;

    public List<PlayerInformation> PlayersList
    {
        get { return playersInformation; }
    }

    public int LobbyId
    {
        get { return lobbyId; }
    }

    public int PlayersInLobby
    {
        get { return playersInformation.Count; }
    }

    public List<ulong> ReadyPlayers
    {
        get { return readyPlayers; }
    }

    public int NumberOfReadyPlayers
    {
        get { return readyPlayers.Count; }
    }

    public bool IsStarted
    {
        get { return isStarted; }
        set { isStarted = value; }
    }

    public bool IsPrivate
    {
        get { return isPrivate; }
    }

    public int RoundNumber
    {
        get { return roundNumber;}
        set { roundNumber = value; }
    }

    public int RoundTime
    {
        get { return roundTime; }
        set { roundTime = value; }
    }


    public Lobby(PlayerInformation creator, int idLobby, LobbyManager lobbyManager)
    {
        //ACTUALIZAR VALORES EN PLAYER INFORMATION
        creator.IdInLobby = 0;
        creator.CurrentLobbyId = idLobby;
        playersInformation.Add(creator);

        this.lobbyId = idLobby;
        this.lobbyManager = lobbyManager;
        this.onlinePlayers = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<OnlinePlayers>();

 
    }

    /*
    public void SuscribirEliminar()
    {
        lobbyManager.LobbyEliminated += EliminarLobby;
    }

    private void EliminarLobby(object sender, EventArgs e)
    {


        lobbyManager.LobbyEliminated -= EliminarLobby;
    }*/

    public bool AddPlayerToLobby(ulong playerId)
    {
        if (playersInformation.Count >= MAX_PLAYERS) return false;

        foreach (var players in playersInformation)
        {
            if (players.Id == playerId)
                return false;
        }

        //ACTUALIZAR VALORES DE PLAYER INFORMATION
        PlayerInformation player = onlinePlayers.ReturnPlayerInformation(playerId);
        player.CurrentLobbyId = lobbyId;
        player.IdInLobby = playersInformation.Count;

        playersInformation.Add(player);
        return true;
    }

    public void RemovePlayerFromLobby(ulong playerId)
    {
        PlayerInformation player = onlinePlayers.ReturnPlayerInformation(playerId);

        //ACTUALIZAR VALOR DE LOS ID EN LOBBY A PARTIR DEL ELIMINADO
        for (int i = player.IdInLobby + 1; i < playersInformation.Count; i++)
        {
            playersInformation[i].IdInLobby--;
        }

        playersInformation.RemoveAt(player.IdInLobby);

        //PONER POR DEFECTO LOS VALORES DE LOBBY EN PLAYER INFORMATION
        player.ResetAfterExitingLobby();

        Debug.Log($"ELIMINADO A {playerId}");
    }

    public void PlayerReady(ulong playerId)
    {
        foreach (var id in readyPlayers)
        {
            if (id == playerId)
            {
                Debug.Log("YA NO LISTO: " + id);
                readyPlayers.Remove(id);
                return;
            }
        }

        readyPlayers.Add(playerId);
        foreach (var player in readyPlayers)
        {
            Debug.Log("LISTO:" + player);
        }
    }

    public bool IsPlayerReady(ulong playerId)
    {
        foreach (var id in readyPlayers)
        {
            if (id == playerId)
                return true;
        }

        return false;
    }

    public List<ulong> GetPlayersIdsList()
    {
        List<ulong> ids = new List<ulong>();
        foreach (var player in playersInformation)
        {
            ids.Add(player.Id);
        }
        return ids;
    }

    public void RemoveAllPlayers()
    {

        while (playersInformation.Count > 0)
        {
            RemovePlayerFromLobby(playersInformation[0].Id);
        }

        readyPlayers.Clear();

        isStarted = false;
        isPrivate = false;
    }
}
