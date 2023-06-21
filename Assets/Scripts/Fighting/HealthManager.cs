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
    public EventHandler<GameObject> ResetHealthEvent;
    public EventHandler<GameObject> DmgTakenEvent;

    private void Start()
    {
        character = GetComponent<FighterMovement>();
        if (!character)
        {
            Debug.Log("ERROR EN CHARACTER_HEALTH: NO HAY PERSONAJE");
            ResetHealth();
        }
    }

    public void ResetHealth()
    {
        healthPoints = maxHealth;
        ResetHealthEvent?.Invoke(this, gameObject);
    }

    public void TakeDmg(int dmg)
    {
        healthPoints -= dmg;
        Debug.Log("Me han pegado");
        if (healthPoints <= 0)
        {
            Debug.Log("Me he muerto");
            healthPoints = 0;
            new DieCommand(character).Execute();
        }

        DmgTakenEvent?.Invoke(this, gameObject); //DESDE AQUI QUIERO ACTUALIZAR LAS VIDAS
    }
}

