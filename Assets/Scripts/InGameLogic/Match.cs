using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class Match
{
    Lobby lobby;
    List<PlayerInformation> players;
    List<Round> roundList = new List<Round>();
    Round playing_round;


    MatchManager matchManager;

    int idLobby;
    int MAX_ROUNDS;
    int current_round;
    int time_per_round;

    public Lobby Lobby
	{
		get { return lobby; }
	}

    public List<PlayerInformation> Players
	{
        get { return players; }
        set { players = value; }
	}

    public Round Playing_Round
    {
        get { return playing_round; }
    }

    public int IdLobby
	{
        get { return idLobby; }
        set { idLobby = value; }
	}

    
    public Match(Lobby lobby, int n_rounds, int time_per_round, MatchManager matchManager)
	{
        Debug.Log("He empezado la partida.");
        idLobby = lobby.LobbyId;
        players = lobby.PlayersList;
        this.lobby = lobby;

        MAX_ROUNDS = n_rounds;
        this.time_per_round = time_per_round;
        this.matchManager = matchManager;

        current_round = 0;

        StartRoundFromMatch();
    }
    
    void StartRoundFromMatch()
	{
        Round round = new Round(this, players, time_per_round);

        playing_round = round;
        matchManager.AddEventMatch(playing_round.Timer);
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

        if (current_round != MAX_ROUNDS)
		{
            StartRoundFromMatch();
            Debug.Log("Empezando siguiente ronda...");
        }
        else
		{
            PrintWinnersFromRounds();
            Debug.Log("Fin de la partida.");
        }
    }

    void PrintWinnersFromRounds()
	{
        int i = 0;
        foreach(var round in roundList)
		{
            i++;
            string text;
            if (round.Winner == null)
			{
                text = $"Ronda {i}, ha empatado.";
            } else
			{
                string ussername = round.Winner.GetComponent<PlayerInformation>().Username;
                text = $"Ronda {i}, ganador: {ussername}";
            }

            Debug.Log(text);
            PrintClientRpc(text);
		}
	}

    [ClientRpc]
    void PrintClientRpc(string text)
	{
        
	}

}
