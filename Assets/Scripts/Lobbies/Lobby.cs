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
    private bool isStarted = false; //sirve para que nadie entre al lobby cuando empiece una partida
    private int roundNumber = 1;
    private int roundTime = 5;
    private string password = "";


    private List<PlayerInformation> playersInformation = new(); //lista de la informacion de los jugadores que hay en la sala
    private const int MAX_PLAYERS = 4;
    private List<ulong> readyPlayers = new();   //lista empleada para la gestion de los jugadores listos

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
        get { return roundNumber; }
        set { roundNumber = value; }
    }

    public int RoundTime
    {
        get { return roundTime; }
        set { roundTime = value; }
    }

    public string Password
    {
        get { return password; }
    }

    //constructor para crear lobby publica
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

    //constructor para lobby privada
    public Lobby(PlayerInformation creator, int idLobby, LobbyManager lobbyManager, string password)
    {
        //ACTUALIZAR VALORES EN PLAYER INFORMATION
        creator.IdInLobby = 0;
        creator.CurrentLobbyId = idLobby;
        playersInformation.Add(creator);
        this.isPrivate = true;

        this.lobbyId = idLobby;
        this.lobbyManager = lobbyManager;
        this.onlinePlayers = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<OnlinePlayers>();
        this.password = password;
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

    //metodo que intenta añadir jugador a la sala, falla si esta llena o el jugador ya esta dentro
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


    // metodo que elimina a un jugador especificado de la sala
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

    }

    //metodo que gestiona cuando un jugador esta listo, si NO estaba ready lo pone a ready y viceversa
    public void PlayerReady(ulong playerId)
    {
        foreach (var id in readyPlayers)
        {
            if (id == playerId)
            {

                readyPlayers.Remove(id);
                return;
            }
        }

        readyPlayers.Add(playerId);
    }

    //devuelve si un jugador esta ready o no
    public bool IsPlayerReady(ulong playerId)
    {
        foreach (var id in readyPlayers)
        {
            if (id == playerId)
                return true;
        }

        return false;
    }

    //metodo que devuelve una lista con los ids de los jugadores de la sala
    public List<ulong> GetPlayersIdsList()
    {
        List<ulong> ids = new List<ulong>();
        foreach (var player in playersInformation)
        {
            ids.Add(player.Id);
        }
        return ids;
    }

    //metodo que elimina a todos los jugadores de la sala, restablenciendola a su estado original
    public void RemoveAllPlayers()
    {

        foreach (var player in playersInformation)
        {
            player.ResetAfterExitingLobby();
            if (player.FighterObject.GetComponent<FighterInformation>().IsDisconnected)
            {
                onlinePlayers.OnlinePlayersDictionary.Remove(player.Id);
            }
        }

        readyPlayers.Clear();
        playersInformation.Clear();

        isStarted = false;
        isPrivate = false;
    }
}
