using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Movement.Components;

public class Match
{
    Lobby lobby;
    ClientRpcParams clientRpcParams;
    List<Vector3> posicionesIniciales = new();

    List<PlayerInformation> players;
    PlayerInformation player_winner;
    List<Round> roundList = new List<Round>();
    Round playing_round;


    MatchManager matchManager;

    int idLobby;
    int MAX_ROUNDS;
    int current_round;
    int time_per_round;

    public EventHandler<Match> StartMatch;
    public EventHandler<Match> EndMatchEvent;

    public Lobby Lobby { get { return lobby; } }

    public List<PlayerInformation> Players
	{
        get { return players; }
        set { players = value; }
	}

    public PlayerInformation Player_Winner
    {
        get { return player_winner; }
        set { player_winner = value; }
    }

    public Round Playing_Round { get { return playing_round; } }

    public MatchManager MatchManager { get { return matchManager; } }

    public int IdLobby
	{
        get { return idLobby; }
        set { idLobby = value; }
	}

    public Match(Lobby lobby, int n_rounds, int time_per_round, MatchManager matchManager, List<Transform> transformIniciales )
	{
        Debug.Log("He empezado la partida.");
        idLobby = lobby.LobbyId;
        players = lobby.PlayersList;

        clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = lobby.GetPlayersIdsList()
            }
        };

        for(int i = 0; i < players.Count; i++)
        {
            GameObject fighter = players[i].FighterObject;
            for (int j = 0; j < players.Count; j++)
            {
                new ActivateCharacterCommand(fighter.GetComponent<FighterMovement>()).Execute(players[j].Id);
            }
        }

        foreach (var player in players)
        {
        }

        this.lobby = lobby;

        MAX_ROUNDS = n_rounds;
        this.time_per_round = time_per_round;

        matchManager.AddEventStartMatch(this);

        StartMatch?.Invoke(this, this);

        this.matchManager = matchManager;

        current_round = 0;

        foreach(var currentTransform in transformIniciales)
        {
            posicionesIniciales.Add(currentTransform.position);
        }
        StartRoundFromMatch();
    }

    void StartRoundFromMatch()
	{
        Round round = new Round(this, players, time_per_round, posicionesIniciales);

        playing_round = round;

        matchManager.AddEventTimerMatch(playing_round.Timer);
        playing_round.StartRound();

        roundList.Add(playing_round);
    }

    public void EndRound(Round round)
	{
        Debug.Log("Se ha terminado la ronda.");
        if (!round.Draw)
            current_round++;
        else
            Debug.Log($"Ronda {current_round} empatada.");

        int desconectados = DesconectadosInGame();

        if ((desconectados == players.Count - 1) || (current_round == MAX_ROUNDS))
        {
            EndMatch();
        } 
        else if (desconectados == players.Count)
        {
            lobby.RemoveAllPlayers();
            matchManager.Destroy(this);
        }
        else 
		{
            StartRoundFromMatch();
            Debug.Log("Empezando siguiente ronda...");
        }
    }

    public int DesconectadosInGame()
    {
        int desconectados = 0;
        foreach (var player in players)
        {
            FighterInformation fighterInformation = player.FighterObject.GetComponent<FighterInformation>();
            if (fighterInformation.IsDisconnected) desconectados++;
        }

        return desconectados;
    }

    public void EndMatch()
    {
        player_winner = GetPlayerWinner();
        matchManager.AddEventEndMatch(this);

        
        EndMatchEvent?.Invoke(this, this);

        Debug.Log("Fin de la partida.");
    }

    PlayerInformation GetPlayerWinner()
	{
        PlayerInformation winner = null;

        int max_ganadas = 0;

        //////////////////////////////TERMINAR PARTIDA PUNTOS//////////////////////////////////////////
        foreach (var player in players)
        {
            FighterInformation fighterInformation = player.FighterObject.GetComponent<FighterInformation>();

            if (!fighterInformation.IsDisconnected)
            {
                int ganadas = fighterInformation.WinnedRounds;

                if (max_ganadas < ganadas)
                {
                    max_ganadas = ganadas;
                    winner = player;    //EN CASO DE QUE NO VAYA, COMENTAR ESTA LINEA Y DESCOMENTAR EL CHORIZO DE ABAJO
                }
            } else
            {
                Debug.Log("Jugador desconectado");
            }
        }

        return winner;
	}
}
