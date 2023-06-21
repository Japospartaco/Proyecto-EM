using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//CLASE DE APOYO PARA FACILITAR LA GESTION DE LOS CLIENTES, LOS LUCHADORES Y LAS PARTIDAS
public class FighterInformation : MonoBehaviour
{
    [Header("Informacion del jugador")]
    [SerializeField] private GameObject player;     //referencia al objeto del cliente que lo posee
    [SerializeField] bool isDisconnected = false; // CUANDO CLIENTE SE DESCONECTE NO QUEREMOS QUE REVIVA

    [Header("Informacion del fighter")]
    [SerializeField] int winnedRounds = 0;      //variable usada para la gestion del ganador de una partida


    public GameObject Player
    {
        get { return player; }
        set { player = value; }
    }

    public int WinnedRounds
    {
        get { return winnedRounds; }
        set { winnedRounds = value; }
    }

    public bool IsDisconnected
    {
        get { return isDisconnected; }
        set { isDisconnected = value; }
    }

}
