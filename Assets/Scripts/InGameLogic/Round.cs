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
    ClientRpcParams clientRpcParams;

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

    public ClientRpcParams ClientRpcParams
	{
        get { return clientRpcParams; }
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

        List<ulong> idPlayers = new();
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
            idPlayers.Add(player.Id);

            contador++;
		}


        this.time_per_round = time_per_round;
        timer = new CountdownTimer(PRE_TIMER, this);

        timer.Alarm += RealStartRound;

        draw = false;
        this.match = match;


        /*
        for (int i = 0; i < fighters.Count; i++)
		{

            fighters[i].GetComponent<FighterMovement>().Move(IMoveableReceiver.Direction.Right);
		}*/


        clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = idPlayers
            }
        };

        StartRound();
    }

    public void StartRound()
	{
        timer.StartTimer();
        Debug.Log("Preparando ronda.");
	}

    public void RealStartRound(object sender, EventArgs eventArgs)
    {
        foreach(var player in fighters)
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
                winner = fighter;
                ganadores++;
            }
        }
        if (ganadores > 1)
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

        if(fighters_alive.Count == 1)
        {
            EndRoundByLastOne();
		}
	}


	private void EndRoundByLastOne()
	{
        Debug.Log("Se ha acabado la ronda por vidas.");
        timer.Alarm -= EndRoundByTimer;

		if (fighters_alive[0] != null)
		{
			if (fighters_alive[0].GetComponent<HealthManager>().healthPoints > 0)
			{
				winner = fighters_alive[0];
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
        foreach (var player in fighters_alive)
        {
            player.GetComponent<HealthManager>().Reset();
        }
        foreach (var player in fighters_dead)
        {
            Debug.Log($"Intentando resucitar a {player.GetComponent<FighterInformation>().Player.GetComponent<PlayerInformation>().Username}");

            new ReviveCommand(player.GetComponent<FighterMovement>()).Execute();
        }
    }

}