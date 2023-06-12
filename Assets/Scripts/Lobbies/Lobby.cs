using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditor;
using UnityEngine;

public class Lobby
{
    private List<PlayerInformation> playersInformation = new();
    private int lobbyId = -1;
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

        //PONER POR DEFECTO LOS VALORES DE LOBBY EN PLAYER INFORMATION
        playersInformation[player.IdInLobby].ResetAfterExitingLobby();

        playersInformation.RemoveAt(player.IdInLobby);
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
}
