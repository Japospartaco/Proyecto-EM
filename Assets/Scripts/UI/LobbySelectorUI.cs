using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class LobbySelectorUI : NetworkBehaviour
{
    [SerializeField] private GameObject LobbySelectorUIObject;

    [SerializeField] private List<GameObject> lobbiesUIObjects;

    [SerializeField] private List<Button> joinButtons;
    [SerializeField] private List<TMP_Text> joinButtonTexts;
    [SerializeField] private List<TMP_Text> numPlayerstexts;

    [SerializeField] private Button createLobbyButton;



    private LobbyManager lobbyManager;

    private void Start()
    {
        LobbySelectorUIObject.SetActive(false);

        lobbyManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<LobbyManager>();

        joinButtons[0].onClick.AddListener(OnJoin0Pressed);
       // joinButtons[1].onClick.AddListener(OnJoin1Pressed);
       // joinButtons[2].onClick.AddListener(OnJoin2Pressed);
       // joinButtons[3].onClick.AddListener(OnJoin3Pressed);

        createLobbyButton.onClick.AddListener(OnCreateLobbyPressed);
    }

    public void OnJoin0Pressed()
    {
        lobbyManager.AddPlayerToLobby(0, NetworkManager.LocalClientId);
        UpdateUIServerRpc();
    }

    public void OnJoin1Pressed()
    {

        lobbyManager.AddPlayerToLobby(1, NetworkManager.LocalClientId);
        UpdateUIServerRpc();
    }

    public void OnJoin2Pressed()
    {

        lobbyManager.AddPlayerToLobby(2, NetworkManager.LocalClientId);
        UpdateUIServerRpc();
    }

    public void OnJoin3Pressed()
    {

        lobbyManager.AddPlayerToLobby(3, NetworkManager.LocalClientId);
        UpdateUIServerRpc();
    }

    public void OnCreateLobbyPressed()
    {
 
        CreateLobbyServerRpc(NetworkManager.LocalClientId);
        Debug.Log("CLIENTE: quiero crear sala");

    }

    public void OnClientJoin()
    {
        if (!IsServer) return;
        Debug.Log("SERVER: cliente se ha unido");
        UpdateUIClientRpc(lobbyManager.Lobbies.Count);
    }

    [ServerRpc(RequireOwnership = false)]
    public void CreateLobbyServerRpc(ulong clientId)
    {
        Debug.Log("SERVER: creando sala");
        lobbyManager.CreateLobby(clientId);
        Debug.Log("SERVER: num de salas actual: " + lobbyManager.Lobbies.Count);
        UpdateUIClientRpc(lobbyManager.Lobbies.Count);
    }

    //METODO QUE RECORRE LA LISTA DE TEXTOS ACTIVOS PARA ACTUALIZAR SUS VALORES
    [ServerRpc(RequireOwnership = false)]
    public void UpdateUIServerRpc()
    {
        UpdateUIClientRpc(lobbyManager.Lobbies.Count);
    }

    [ClientRpc]
    public void UpdateUIClientRpc(int currentActiveLobbies)
    {
        for (int i = 0; i < currentActiveLobbies; i++)
        {
            Debug.Log("CLIENTE: actualizando UI");
            lobbiesUIObjects[i].SetActive(true);
            joinButtonTexts[i].text = $"Lobby {lobbyManager.Lobbies[i].LobbyId}";
            numPlayerstexts[i].text = $"{lobbyManager.Lobbies[i].PlayersInLobby}/4";
        }
    }
}
