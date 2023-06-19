using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//########### EN PRINCIPIO NO SE USA #############
public class FighterInformation : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] int winnedRounds = 0;
    [SerializeField] bool isDisconnected = false; // CUANDO CLIENTE SE DESCONECTE NO QUEREMOS QUE REVIVA

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
