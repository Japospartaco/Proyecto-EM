using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Match
{
    List<PlayerInformation> players;
    List<Round> roundList = new List<Round>();

    int idLobby;
    int MAX_ROUNDS;
    int current_round;
    int time_per_round;
    
    public Match(Lobby lobby, int n_rounds, int time_per_round)
	{
        Debug.Log("He empezado la partida.");
        idLobby = lobby.LobbyId;
        players = lobby.PlayersList;

        foreach (var player in players)
		{
            Debug.Log($"Hola, soy {player.Username}");
		}

        MAX_ROUNDS = n_rounds;
        this.time_per_round = time_per_round;

        current_round = 0;

        StartRoundFromMatch();
    }
    
    void StartRoundFromMatch()
	{
        Round round = new Round(this, players, time_per_round);
        round.StartRound();
        roundList.Add(round);

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
