using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//########### EN PRINCIPIO NO SE USA #############
public class FighterInformation : MonoBehaviour
{
    [SerializeField] private GameObject player;

    public GameObject Player
    {
        get { return player; }
        set { player = value; }
    }
}
