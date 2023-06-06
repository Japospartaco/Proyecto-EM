using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Match
{
    List<ulong> id_players;
    OnlinePlayers onlinePlayers;
    List<Round> roundList;

    int idRoom;
    int MAX_ROUNDS;
    int current_round;
    int time_per_round;
    
    public Match(OnlinePlayers onlinePlayers, List<PlayerInformation> playerList, int n_rounds, int idRoom, int time_per_round)
	{
        foreach(var player in playerList)
		{
            id_players.Add(player.Id);
		}

        this.onlinePlayers = onlinePlayers;

        this.idRoom = idRoom;
        MAX_ROUNDS = n_rounds;
        this.time_per_round = time_per_round;
        current_round = 0;

    }
    
    void StartRoundFromMatch()
	{
        Round round = new Round(this, onlinePlayers, id_players, time_per_round);
        roundList.Add(round);
        round.StartRound();
	}

    public void EndRound(Round round)
	{
        if (!round.Draw)
            current_round++;

        if (current_round != MAX_ROUNDS)
            StartRoundFromMatch();
        else
            PrintWinnersFromRounds();
	}

    void PrintWinnersFromRounds()
	{
        int i = 0;
        foreach(var round in roundList)
		{
            i++;
            string ussername = round.Winner.GetComponent<PlayerInformation>().Username;
            PrintClientRpc(i, ussername);
		}
	}

    [ClientRpc]
    void PrintClientRpc(int n_ronda, string winner)
	{
        string text = $"Ronda {n_ronda}, ganador: {winner}";

	}

}
