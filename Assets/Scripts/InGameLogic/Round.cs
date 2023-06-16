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

    public Round(Match match, List<PlayerInformation> players, float time)
    {
        Debug.Log("Ronda creada :D");
        this.players = players;

        //List<ulong> idPlayers = new();

        foreach (var player in players)
		{
            Debug.Log($"{player.Username} ha seleccionado a {player.FighterObject}");
            fighters.Add(player.FighterObject);
            //idPlayers.Add(player.Id);
		}

        timer = new CountdownTimer(time);

        timer.Alarm += EndRoundByTimer;

        draw = false;
        this.match = match;

        GameObject initPos = GameObject.FindGameObjectWithTag("Spawn positions");


        for (int i = 0; i < fighters.Count; i++)
		{
            Vector3 InitPos = initPos.transform.GetChild(i).position;

            fighters[i].transform.position = InitPos;
            //fighters[i].GetComponent<FighterMovement>().Move(IMoveableReceiver.Direction.Right);
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

    private void PrepareForNextRound()
	{
        RestoreAll();
        if (winner != null)
		{
            winner = winner.GetComponent<FighterInformation>().Player;
		}
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
            player.GetComponent<FighterMovement>().Revive();
        }
    }

}