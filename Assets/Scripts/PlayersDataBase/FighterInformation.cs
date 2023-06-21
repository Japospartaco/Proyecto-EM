using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//########### EN PRINCIPIO NO SE USA #############
public class FighterInformation : MonoBehaviour
{
    [Header("Informacion del jugador")]
    [SerializeField] private GameObject player;
    [SerializeField] bool isDisconnected = false; // CUANDO CLIENTE SE DESCONECTE NO QUEREMOS QUE REVIVA

    [Header("Informacion del fighter")]
    [SerializeField] int winnedRounds = 0;


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
