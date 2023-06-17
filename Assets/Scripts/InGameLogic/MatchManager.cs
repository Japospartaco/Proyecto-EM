using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MatchManager : NetworkBehaviour
{
    [SerializeField] List<Match> matchList = new();
    [SerializeField] MatchUI matchUI;

    public List<Match> MatchList
    {
        get { return matchList; }
    }

    public void AddMatch(Match match)
    {
        matchList.Add(match);
    }

    public void AddEventMatch(CountdownTimer countdownTimer)
	{
        matchUI.SuscribirTiempo(countdownTimer);
    }

    public Match ReturnMatch(Match match)
    {
        foreach (var currentMatch in matchList)
        {
            if (match == currentMatch)
                return currentMatch;
        }

        return null;
    }

    public Match ReturnMatchFromIdLobby(int idLobby)
    {
        foreach (var match in matchList)
        {
            if (match.IdLobby == idLobby)
                return match;
        }

        return null;
    }
}
