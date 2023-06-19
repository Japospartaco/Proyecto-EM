using Movement.Commands;
using Movement.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Round
{
    CountdownTimer timer;

    List<PlayerInformation> players;

    List<GameObject> fighters = new List<GameObject>();
    List<GameObject> fighters_alive = new List<GameObject>();
    List<GameObject> fighters_dead = new List<GameObject>();

    GameObject winner;

    Match match;

    bool draw;

    float PRE_TIMER = 3.0f;
    float time_per_round;

    public CountdownTimer Timer
    {
        get { return timer; }
    }

    public bool Draw
    {
        get { return draw; }
    }

    public GameObject Winner
    {
        get { return winner; }
    }

    public Match Match
    {
        get { return match; }
    }


    public Round(Match match, List<PlayerInformation> players, float time_per_round)
    {
        Debug.Log("Ronda creada :D");
        this.players = players;

        GameObject initPos = GameObject.FindGameObjectWithTag("Spawn positions");

        int contador = 0;
        foreach (var player in players)
        {
            GameObject fighter = player.FighterObject;
            Vector3 InitPos = initPos.transform.GetChild(contador).position;

            fighter.transform.position = InitPos;

            FighterMovement fighterMovement = fighter.GetComponent<FighterMovement>();

            fighterMovement.AllowedMovement = false;
            fighterMovement.DieEvent += FighterDies;

            fighters.Add(fighter);

            contador++;
        }


        this.time_per_round = time_per_round;
        timer = new CountdownTimer(PRE_TIMER, this);

        timer.Alarm += RealStartRound;

        draw = false;

        this.match = match;
    }

    public void StartRound()
    {
        timer.StartTimer();
        Debug.Log("Preparando ronda.");
    }

    public void RealStartRound(object sender, EventArgs eventArgs)
    {
        foreach (var player in fighters)
        {
            player.GetComponent<FighterMovement>().AllowedMovement = true;
        }

        timer.Timer = time_per_round;

        timer.Alarm -= RealStartRound;
        timer.Alarm += EndRoundByTimer;

        timer.StartTimer();

        foreach (var player in fighters)
        {
            fighters_alive.Add(player);
        }
    }

    private void EndRoundByTimer(object sender, EventArgs e)
    {
        Debug.Log("Se ha acabado la ronda por tiempo.");
        int ganadores = 0;
        int maxHp = 0;

        foreach (var fighter in fighters)
        {
            int currentHp = fighter.GetComponent<HealthManager>().healthPoints;

            if (currentHp > maxHp)
            {
                maxHp = currentHp;
            }
        }

        foreach (var fighter in fighters)
        {
            int hp = fighter.GetComponent<HealthManager>().healthPoints;

            if (hp == maxHp)
            {
                ganadores++;
            }
        }

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



    public void FighterDies(object sender, GameObject fighter)
    {
        Debug.Log("Se ha muerto");
        //ESTA LINEA DESDE FIGHTER MOVEMENT
        if (fighters_alive.Remove(fighter))
            fighters_dead.Add(fighter);

        if (fighters_alive.Count == 1)
        {
            EndRoundByLastOne();
        }
    }


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
            else
            {
                //ESTA MUERTO
            }
        }
        else
        {
            //ES NULL
        }

        PrepareForNextRound();
    }

    private void PrepareForNextRound()
    {
        timer.ResetTimer();

        RestoreAll();
        if (winner != null)
        {
            winner = winner.GetComponent<FighterInformation>().Player;
        }
        Debug.Log($"Ganador de la ronda: {winner}");
        match.EndRound(this);
    }

    private void RestoreAll()
    {
        foreach (var player in fighters)
        {
            player.GetComponent<FighterMovement>().DieEvent -= FighterDies;
        }

        foreach (var player in fighters_alive)
        {
            player.GetComponent<HealthManager>().Reset();
        }
        foreach (var player in fighters_dead)
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = match.Lobby.GetPlayersIdsList()
                }
            };

            //Si DoNotResuscitate es true no se revive al personaje
            if (!player.GetComponent<FighterInformation>().DoNotResuscitate)
                new ReviveCommand(player.GetComponent<FighterMovement>()).Execute(clientRpcParams);
        }
    }

}