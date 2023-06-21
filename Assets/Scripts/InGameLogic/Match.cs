using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Movement.Components;

public class Match
{
    // Variables de instancia
    Lobby lobby; // Representa el lobby de la partida
    List<Vector3> posicionesIniciales = new(); // Lista de posiciones iniciales de los jugadores

    List<PlayerInformation> players; // Lista de información de jugadores
    PlayerInformation player_winner; // Información del jugador ganador
    List<Round> roundList = new List<Round>(); // Lista de rondas de la partida
    Round playing_round; // Ronda actual

    MatchManager matchManager; // Administrador de la partida

    int idLobby; // ID del lobby
    int MAX_ROUNDS; // Número máximo de rondas
    int current_round; // Ronda actual
    int time_per_round; // Tiempo por ronda

    public EventHandler<Match> StartMatch; // Evento de inicio de partida
    public EventHandler<Match> EndMatchEvent; // Evento de final de partida

    // Propiedades de la clase partida
    public Lobby Lobby { get { return lobby; } }
    public List<PlayerInformation> Players { get { return players; } set { players = value; } }
    public PlayerInformation Player_Winner { get { return player_winner; } set { player_winner = value; } }
    public Round Playing_Round { get { return playing_round; } }
    public MatchManager MatchManager { get { return matchManager; } }
    public int IdLobby { get { return idLobby; } set { idLobby = value; } }

    // Constructor de la partida
    public Match(Lobby lobby, int n_rounds, int time_per_round, MatchManager matchManager, List<Transform> transformIniciales)
    {
        // Inicialización de variables
        Debug.Log("He empezado la partida.");
        idLobby = lobby.LobbyId;
        players = lobby.PlayersList;

        // Activa los personajes (Fighters) solo para los jugadores en el lobby
        for (int i = 0; i < players.Count; i++)
        {
            GameObject fighter = players[i].FighterObject;
            for (int j = 0; j < players.Count; j++)
            {
                new ActivateCharacterCommand(fighter.GetComponent<FighterMovement>()).Execute(players[j].Id);
            }
        }

        this.lobby = lobby;

        MAX_ROUNDS = n_rounds;
        this.time_per_round = time_per_round;

        // Suscribe al evento que inicializa la interfaz de usuario
        matchManager.AddEventStartMatch(this);

        StartMatch?.Invoke(this, this);

        this.matchManager = matchManager;

        current_round = 0;

        foreach (var currentTransform in transformIniciales)
        {
            posicionesIniciales.Add(currentTransform.position);
        }
        StartRoundFromMatch();
    }

    // Comienza una ronda desde la partida
    void StartRoundFromMatch()
    {
        Round round = new Round(this, players, time_per_round, posicionesIniciales);

        playing_round = round;

        matchManager.AddEventTimerMatch(playing_round.Timer);
        playing_round.StartRound();

        roundList.Add(playing_round);
    }

    // Gestiona el final de una ronda
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

    // Verifica la cantidad de jugadores desconectados en el juego al finalizar una ronda
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

    // Finaliza la partida y muestra la interfaz posterior a la partida
    public void EndMatch()
    {
        player_winner = GetPlayerWinner();
        matchManager.AddEventEndMatch(this);


        EndMatchEvent?.Invoke(this, this);

        Debug.Log("Fin de la partida.");
    }

    // Obtiene la información del jugador ganador
    PlayerInformation GetPlayerWinner()
    {
        PlayerInformation winner = null;

        int max_ganadas = 0;

        // Terminar la partida por puntos
        foreach (var player in players)
        {
            FighterInformation fighterInformation = player.FighterObject.GetComponent<FighterInformation>();

            if (!fighterInformation.IsDisconnected)
            {
                int ganadas = fighterInformation.WinnedRounds;

                if (max_ganadas < ganadas)
                {
                    max_ganadas = ganadas;
                    winner = player; // En caso de que no funcione, comentar esta línea y descomentar la siguiente
                }
            }
            else
            {
                Debug.Log("Jugador desconectado");
            }
        }

        return winner;
    }
}
