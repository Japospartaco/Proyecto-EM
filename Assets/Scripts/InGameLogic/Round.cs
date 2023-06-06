using Movement.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Round
{
    CountdownTimer timer;

    //List<ulong> id_players;
    List<GameObject> fighters;
    List<GameObject> fighters_alive;
    List<GameObject> fighters_dead;
    GameObject winner;
    bool draw;

    Match match;

    public bool Draw
	{
        get { return draw; }
	}

    public GameObject Winner
    {
        get { return winner; }
    }

    public Round(Match match, OnlinePlayers onlinePlayers, List<ulong> id_players, float time)
    {

        //this.id_players = id_players;
        fighters = onlinePlayers.GetFighterListFromIds(id_players);
        timer = new CountdownTimer(time);
        fighters_dead = new List<GameObject>();
        fighters_alive = new List<GameObject>();
        timer.Alarm += EndRoundByTimer;
        draw = false;
        this.match = match;

        GameObject initPos = GameObject.FindGameObjectWithTag("Init Pos");

        for (int i = 0; i < fighters.Count; i++)
		{
            fighters[i].transform.position = initPos.transform.GetChild(i).position;
		}
    }

    public void StartRound()
    {
        timer.StartTimer();

        foreach (var player in fighters)
        {
            fighters_alive.Add(player);
        }
    }

    //public void CheckEndRound()
    //{
    //    bool endTimer = timer.Timer <= 0;
    //    bool endAlives = fighters_alive.Count == 1;
    //    int max_vit = 0;
    //
    //
    //    if (endTimer || endAlives)
    //    {
    //        foreach (var player in fighters)
    //        {
    //            int hp = player.GetComponent<HealthManager>().vit;
    //
    //            if (hp > max_vit)
    //            {
    //                max_vit = hp;
    //            }
    //            if (hp <= 0)
    //            {
    //                fighters_dead.Add(player);
    //                fighters_alive.Remove(player);
    //            }
    //        }
    //
    //        if (endTimer) EndRoundByTimer(max_vit);
    //        if (endAlives) EndRoundByLastOne();
    //        RestoreAll();
    //    }
    //
    //}


    private void EndRoundByTimer(object sender, EventArgs e)
    {
        int ganadores = 0;
        int maxHp = 0;

        foreach (var player in fighters)
        {
            int currentHp = player.GetComponent<HealthManager>().healthPoints;

            if (currentHp > maxHp)
            {
                maxHp = currentHp;
            }
        }

        foreach (var player in fighters)
        {
            int hp = player.GetComponent<HealthManager>().healthPoints;

            if (hp == maxHp)
            {
                winner = player;
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

    public void FighterDies(object sender, int idInLobby)
    {
        //ESTA LINEA DESDE FIGHTER MOVEMENT
        //int idInLobby = fighter.GetComponent<FighterInformation>().IdInLobby;
        fighters_dead.Add(fighters[idInLobby]);
        fighters_alive.Remove(fighters[idInLobby]);

        if(fighters_alive.Count == 1)
        {
            EndRoundByLastOne();
		}
	}


	private void EndRoundByLastOne()
	{
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

	private void RestoreAll()
    {
        foreach (var player in fighters_alive)
        {
            player.GetComponent<HealthManager>().Reset();
        }
        foreach (var player in fighters_dead)
        {
            player.GetComponent<FighterMovement>().Revive();
        }
    }

    private void PrepareForNextRound()
	{
        RestoreAll();
        match.EndRound(this);
    }

}