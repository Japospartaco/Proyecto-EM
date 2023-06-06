using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInformation : MonoBehaviour
{
    [SerializeField] private ulong id;
    [SerializeField] private string username;
    [SerializeField] private GameObject fighterObject;
    [SerializeField] private int currentLobbyId;

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

    public void InitializePlayer(ulong id, string username, GameObject fighterObject, int currentLobbyId)
    {
        this.id = id;
        this.username = username;   
        this.fighterObject = fighterObject; 
        this.currentLobbyId = currentLobbyId;
    }


}
