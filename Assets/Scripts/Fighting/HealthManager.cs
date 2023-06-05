using Movement.Commands;
using Movement.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthManager : MonoBehaviour
{
    public int healthPoints;
    public int maxHealth;

    public FighterMovement character;

    public int playerIdInRoom;

    //EVENTOS
    //public DmgEvent DmgTaken = new DmgEvent();
    public EventHandler<bool> Dead;
    public EventHandler<int> DmgTaken;

    private void Start()
    {
        character = GetComponent<FighterMovement>();
        if (!character)
        {
            Debug.Log("ERROR EN CHARACTER_HEALTH: NO HAY PERSONAJE");
            Reset();
        }
    }

    public void Reset()
    {
        healthPoints = maxHealth;
    }

    public void TakeDmg(int dmg)
    {
        healthPoints -= dmg;
        //DmgTaken?.Invoke(healthPoints);

        if (healthPoints <= 0)
        {
            new DieCommand(character).Execute();
            Dead?.Invoke(this, false);
        }
    }


}
public class DmgEvent : UnityEvent<int>
{
}
