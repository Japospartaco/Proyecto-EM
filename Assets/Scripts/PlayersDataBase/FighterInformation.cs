using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//########### EN PRINCIPIO NO SE USA #############
public class FighterInformation : MonoBehaviour
{
    private PlayerInformation playerInformation;
    private int idInLobby;

    public PlayerInformation PlayerInformation
    {
        get { return playerInformation; }
        set { playerInformation = value; }
    }

    public int IdInLobby
    {
        get { return idInLobby; }
        set { idInLobby = value; }
    }
}
