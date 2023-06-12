using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class LobbySelectorUI : NetworkBehaviour
{
    [SerializeField] private GameObject lobbySelectorUIObject;
    [SerializeField] private GameObject fighterSelectorUIObject;

    [SerializeField] private List<GameObject> lobbiesUIObjects;

    [SerializeField] private List<Button> joinButtons;
    [SerializeField] private List<TMP_Text> joinButtonTexts;
    [SerializeField] private List<TMP_Text> numPlayerstexts;

    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button refreshButton;

    private LobbyManager lobbyManager;



    private void Start()
    {
        lobbySelectorUIObject.SetActive(false);

        joinButtons[0].onClick.AddListener(OnJoin0Pressed);
        joinButtons[1].onClick.AddListener(OnJoin1Pressed);
        joinButtons[2].onClick.AddListener(OnJoin2Pressed);
        joinButtons[3].onClick.AddListener(OnJoin3Pressed);

        createLobbyButton.onClick.AddListener(OnCreateLobbyPressed);
        refreshButton.onClick.AddListener(OnRefreshButtonPressed);

        lobbyManager = GameObject.FindWithTag("Game Manager").GetComponent<LobbyManager>();
        if (lobbyManager == null) Debug.Log("LOBBY MANAGER NO ENCONTRAO");
    }

    public void OnRefreshButtonPressed()
    {
        Refresh();
    }

    public void OnJoin0Pressed()
    {
        AddClientToLobbyServerRpc(0, NetworkManager.LocalClientId);
        lobbySelectorUIObject.SetActive(false);
        fighterSelectorUIObject.SetActive(true);

    }

    public void OnJoin1Pressed()
    {
        AddClientToLobbyServerRpc(1, NetworkManager.LocalClientId);
        lobbySelectorUIObject.SetActive(false);
        fighterSelectorUIObject.SetActive(true);

    }

    public void OnJoin2Pressed()
    {
        AddClientToLobbyServerRpc(2, NetworkManager.LocalClientId);
        lobbySelectorUIObject.SetActive(false);
        fighterSelectorUIObject.SetActive(true);

    }

    public void OnJoin3Pressed()
    {
        AddClientToLobbyServerRpc(3, NetworkManager.LocalClientId);
        lobbySelectorUIObject.SetActive(false);
        fighterSelectorUIObject.SetActive(true);

    }

    //METODO QUE UNE A JUGADOR A SALA Y ACTUALIZA INTERFAZ DE TODOS
    [ServerRpc(RequireOwnership = false)]
    public void AddClientToLobbyServerRpc(int lobbyIndex, ulong clientId)
    {
        lobbyManager.AddPlayerToLobby(lobbyIndex, clientId);

        for (int i = 0; i < lobbyManager.Lobbies.Count; i++)
        {
            UpdateUIClientRpc(i, lobbyManager.Lobbies[i].LobbyId, lobbyManager.Lobbies[i].PlayersInLobby);
        }
    }

    public void OnCreateLobbyPressed()
    {

        CreateLobbyServerRpc(NetworkManager.LocalClientId);
        Debug.Log("CLIENTE: quiero crear sala");

    }

    public void Refresh()
    {
        Debug.Log("CLIENTE: actualizando interfaz");

        RefreshServerRpc(NetworkManager.LocalClientId);
    }

    //METODO QUE ACTUALIZA INTERFAZ DE UN UNICO PLAYER
    [ServerRpc(RequireOwnership = false)]
    public void RefreshServerRpc(ulong clientId)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        for (int i = 0; i < lobbyManager.Lobbies.Count; i++)
        {
            UpdateUIClientRpc(i, lobbyManager.Lobbies[i].LobbyId, lobbyManager.Lobbies[i].PlayersInLobby, clientRpcParams);
        }
    }

    //METODO QUE CREA LOBBY Y ACTUALIZA INTERFAZ DE TODOS
    [ServerRpc(RequireOwnership = false)]
    public void CreateLobbyServerRpc(ulong clientId)
    {
        Debug.Log("SERVER: creando sala");
        lobbyManager.CreateLobby(clientId);
        Debug.Log("SERVER: num de salas actual: " + lobbyManager.Lobbies.Count);

        for (int i = 0; i < lobbyManager.Lobbies.Count; i++)
        {
            UpdateUIClientRpc(i, lobbyManager.Lobbies[i].LobbyId, lobbyManager.Lobbies[i].PlayersInLobby);
        }
    }

    //METODO QUE ACTUALIZA INTERFAZ
    [ClientRpc]
    public void UpdateUIClientRpc(int i, int lobbyId, int playersInLobby, ClientRpcParams clientRpcParams = default)
    {

        Debug.Log("CLIENTE: actualizando UI");
        lobbiesUIObjects[i].SetActive(true);
        joinButtonTexts[i].text = $"Lobby {lobbyId + 1}";
        numPlayerstexts[i].text = $"{playersInLobby}/4";
    }
}
