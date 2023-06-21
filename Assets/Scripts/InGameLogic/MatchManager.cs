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

    public void AddEventStartMatch(Match match)
    {
        if (!IsServer) return;
        matchUI.SuscribirInicializarUIHealth(match);
    }

    public void AddEventHealthInterface(HealthManager fighterDamaged)
    {
        if (!IsServer) return;
        matchUI.SuscribirInterfazVidas(fighterDamaged);
    }

    public void AddEventTimerMatch(CountdownTimer countdownTimer)
	{
        if (!IsServer) return;
        matchUI.SuscribirTiempo(countdownTimer);
    }

    public void AddEventEndMatch(Match match)
    {
        if (!IsServer) return;
        matchUI.SuscribirFinPartida(match);
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

    public void Destroy(Match match)
    {
        Debug.Log("Destruyendo la partida.");

        foreach(var player in match.Players)
        {
            Destroy(player.FighterObject);
        }
    }

}
