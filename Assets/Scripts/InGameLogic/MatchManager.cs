using Movement.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MatchManager : NetworkBehaviour
{
    [Header("Clases auxiliares")]
    [SerializeField] MatchUI matchUI; // Referencia al componente MatchUI utilizado para mostrar la interfaz del partido

    List<Match> matchList = new(); // Lista de partidas en el administrador de partidas

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
        matchUI.SuscribirInicializarUIMatch(match); // Suscribirse al evento de inicio del partido para inicializar la interfaz de salud
    }

    public void AddEventFighterMovement(FighterMovement fighterMovement)
    {
        if (!IsServer) return;
        matchUI.SuscribirUIFighterMovement(fighterMovement);
    }

    public void AddEventHealthManager(HealthManager fighterDamaged)
    {
        if (!IsServer) return;
        matchUI.SuscribirUIHealthManager(fighterDamaged); // Suscribirse al evento de interfaz de salud para actualizar la interfaz cuando un luchador recibe daño
    }

    public void AddEventTimerMatch(CountdownTimer countdownTimer)
    {
        if (!IsServer) return;
        matchUI.SuscribirTiempo(countdownTimer); // Suscribirse al evento del temporizador del partido para mostrar el tiempo restante en la interfaz
    }

    public void AddEventEndMatch(Match match)
    {
        if (!IsServer) return;
        matchUI.SuscribirFinPartida(match); // Suscribirse al evento de fin del partido para mostrar los resultados finales en la interfaz
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

        foreach (var player in match.Players)
        {
            Destroy(player.FighterObject); // Destruir los objetos de luchadores asociados a la partida
        }
    }

}
