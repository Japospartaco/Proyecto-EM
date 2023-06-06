using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lobby
{
    private List<PlayerInformation> playersInformation = new();
    private int lobbyId = -1;
    private const int MAX_PLAYERS = 4;

    private OnlinePlayers onlinePlayers;
    private LobbyManager lobbyManager;

    public int LobbyId
    {
        get { return this.lobbyId; }
    }

    public int PlayersInLobby
    {
        get { return playersInformation.Count; }
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
}
