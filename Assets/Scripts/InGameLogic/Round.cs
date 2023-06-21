using Movement.Commands;
using Movement.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Round
{
    CountdownTimer timer; // Temporizador de cuenta regresiva para la ronda
    List<GameObject> fighters = new List<GameObject>(); // Lista de luchadores en la ronda
    List<GameObject> fighters_alive = new List<GameObject>(); // Lista de luchadores vivos en la ronda
    List<GameObject> fighters_dead = new List<GameObject>(); // Lista de luchadores muertos en la ronda
    GameObject winner; // Ganador de la ronda
    Match match; // Referencia al objeto Match al que pertenece la ronda
    bool draw; // Indicador de empate en la ronda
    float PRE_TIMER = 3.0f; // Tiempo previo a la ronda para prepararse
    float time_per_round; // Tiempo total por ronda

    public CountdownTimer Timer { get { return timer; } }
    public bool Draw { get { return draw; } }
    public GameObject Winner { get { return winner; } }
    public Match Match {  get { return match; }
    }

    public Round(Match match, List<PlayerInformation> players, float time_per_round, List<Vector3> posiciones)
    {
        Debug.Log("Ronda creada :D");

        int contador = 0;
        foreach (var player in players)
        {
            GameObject fighter = player.FighterObject;

            fighter.transform.position = posiciones[contador];

            FighterMovement fighterMovement = fighter.GetComponent<FighterMovement>();

            fighterMovement.AllowedMovement = false;
            fighterMovement.DieEvent += FighterDies;

            fighters.Add(fighter);

            contador++;
        }

        this.time_per_round = time_per_round;
        timer = new CountdownTimer(PRE_TIMER, this);

        //Comienzo real de la ronda. Invalida el movimiento de los jugadores durante 3 segundos.
        timer.Alarm += RealStartRound;

        draw = false;

        this.match = match;
    }

    //Metodo llamado desde la clase partida. 
    public void StartRound()
    {
        timer.StartTimer();
        Debug.Log("Preparando ronda.");
    }

    //Una vez ha terminado el temporizador, empieza la verdadera ronda.
    public void RealStartRound(object sender, EventArgs eventArgs)
    {
        // Habilitar el movimiento de los luchadores
        foreach (var player in fighters)
        {
            player.GetComponent<FighterMovement>().AllowedMovement = true;
        }

        timer.CurrentTime = time_per_round;

        timer.Alarm -= RealStartRound;
        timer.Alarm += EndRoundByTimer;

        timer.StartTimer();

        // Clasificar los luchadores vivos y muertos en funcion de su estado de conexion
        foreach (var player in fighters)
        {
            if (player.GetComponent<FighterInformation>().IsDisconnected)
            {
                fighters_dead.Add(player);
            }
            else
            {
                fighters_alive.Add(player);
            }
        }

        // Si solo queda un luchador vivo, terminar la ronda
        if (fighters_alive.Count == 1)
            EndRoundByLastOne();
    }

    //Fin de la ronda por tiempo acabado.
    private void EndRoundByTimer(object sender, EventArgs e)
    {
        Debug.Log("Se ha acabado la ronda por tiempo.");
        int ganadores = 0;
        int maxHp = 0;

        // Buscar el máximo valor de puntos de vida entre todos los luchadores
        foreach (var fighter in fighters)
        {
            int currentHp = fighter.GetComponent<HealthManager>().healthPoints;

            if (currentHp > maxHp)
            {
                maxHp = currentHp;
            }
        }

        // Contar el número de luchadores que tienen el máximo valor de puntos de vida
        foreach (var fighter in fighters)
        {
            int hp = fighter.GetComponent<HealthManager>().healthPoints;

            if (hp == maxHp)
            {
                ganadores++;
            }
        }

        // Determinar al ganador o indicar empate
        if (ganadores == 1)
        {
            foreach (var fighter in fighters)
            {
                int hp = fighter.GetComponent<HealthManager>().healthPoints;

                if (hp == maxHp)
                {
                    winner = fighter;
                    winner.GetComponent<FighterInformation>().WinnedRounds += 1;
                }
            }
        }
        else
        {
            winner = null;
            draw = true;
        }

        PrepareForNextRound();
    }

    //Evento que es invocado cada vez que un jugador muere.
    public void FighterDies(object sender, GameObject fighter)
    {
        Debug.Log("Se ha muerto");

        // Remover al luchador de la lista de vivos y agregarlo a la lista de muertos
        if (fighters_alive.Remove(fighter))
            fighters_dead.Add(fighter);

        // Si solo queda un luchador vivo, terminar la ronda
        if (fighters_alive.Count == 1)
        {
            EndRoundByLastOne();
        }
    }

    //Terminar la ronda porque solo queda un jugador vivo.
    private void EndRoundByLastOne()
    {
        Debug.Log("Se ha acabado la ronda por vidas.");

        if (fighters_alive[0] != null)
        {
            if (fighters_alive[0].GetComponent<HealthManager>().healthPoints > 0)
            {
                winner = fighters_alive[0];
                winner.GetComponent<FighterInformation>().WinnedRounds += 1;
            }
        }

        PrepareForNextRound();
    }

    //Preparar a todos los jugadores para la siguiente ronda.
    private void PrepareForNextRound()
    {
        timer.ResetTimer();

        RestoreAll();

        // Si hay un ganador, obtener la referencia al jugador correspondiente
        if (winner != null)
        {
            winner = winner.GetComponent<FighterInformation>().Player;
        }

        Debug.Log($"Ganador de la ronda: {winner}");

        match.EndRound(this);
    }

    //Restaurar el estado inicial de todos los luchadores.
    private void RestoreAll()
    {
        foreach (var player in fighters)
        {
            player.GetComponent<FighterMovement>().DieEvent -= FighterDies;
        }

        // Restaurar la salud de los luchadores vivos
        foreach (var player in fighters_alive)
        {
            player.GetComponent<HealthManager>().ResetHealth();
        }

        // Revivir a los luchadores muertos y notificar a los clientes
        foreach (var player in fighters_dead)
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = match.Lobby.GetPlayersIdsList()
                }
            };

            // Si el luchador no está desconectado, intentar revivirlo
            if (!player.GetComponent<FighterInformation>().IsDisconnected)
            {
                Debug.Log("Jugador NO desconectado. Intentando revivir.");
                new ReviveCommand(player.GetComponent<FighterMovement>()).Execute(clientRpcParams);
            }
        }
    }
}
