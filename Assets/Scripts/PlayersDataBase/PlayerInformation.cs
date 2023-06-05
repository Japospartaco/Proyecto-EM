using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInformation : MonoBehaviour
{
    private ulong id;
    private string username;
    private GameObject fighterObject;
    private int currentLobbyId;

    public ulong Id
    {
        get { return id; }
        set { id = value; }
    }

    public string Username
    {
        get { return username; }
        set { username = value; }
    }

    public GameObject FighterObject
    {
        get { return fighterObject; }
        set { fighterObject = value; }
    }

    public int CurrentLobbyId
    {
        get { return currentLobbyId; }
        set { currentLobbyId = value; }
    }


}
