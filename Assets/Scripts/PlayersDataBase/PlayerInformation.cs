using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInformation : MonoBehaviour
{
    [SerializeField] private ulong id;
    [SerializeField] private string username;
    [SerializeField] private GameObject fighterObject;
    [SerializeField] private int currentLobbyId;
    [SerializeField] private int idInLobby;
    [SerializeField] private int selectedFighter;

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

    public int IdInLobby
    {
        get { return idInLobby; }
        set { idInLobby = value; }
    }

    public int SelectedFighter
    {
        get { return selectedFighter; }
        set { selectedFighter = value; }
    }

    //METODO PARA INICILIZAR VALORES (SIMILAR A CONSTRUCTOR)
    public void InitializePlayer(ulong id, string username, GameObject fighterObject, int currentLobbyId, int idInLobby)
    {
        this.id = id;
        this.username = username;
        this.fighterObject = fighterObject;
        this.currentLobbyId = currentLobbyId;
        this.idInLobby = idInLobby;
        this.selectedFighter = 0;
    }

    //METODO PARA RESTAURAR LOS VALORES RELACIONADOS CON "LOBBY" AL SALIR DE UNA
    public void ResetAfterExitingLobby()
    {
        currentLobbyId = -1;
        idInLobby = -1;
    }


}
