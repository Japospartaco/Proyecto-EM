using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class LobbySelectorUI : NetworkBehaviour
{
    [Header("UIs utilizadas")]
    [SerializeField] private GameObject lobbySelectorUIObject;
    [SerializeField] private GameObject fighterSelectorUIObject;
    [SerializeField] private GameObject chatUIObject;

    [Header("Objetos con layers utilizadas")]
    [SerializeField] private List<GameObject> lobbiesUIObjects;

    [Header("Gestion de entrada a lobbies")]
    [SerializeField] private List<Button> joinButtons;
    [SerializeField] private List<TMP_Text> joinButtonTexts;
    [SerializeField] private List<TMP_Text> numPlayerstexts;

    [Header("Gestión de creación de lobby")]
    [SerializeField] private Button createPublicLobbyButton;
    [SerializeField] private Button createPrivateLobbyButton;
    [SerializeField] private TMP_InputField passwordInputField;

    [Header("Refresh button")]
    [SerializeField] private Button refreshButton;

    [Header("Clases auxiliares")]
    [SerializeField] LobbyManager lobbyManager;

    private void Start()
    {
        lobbySelectorUIObject.SetActive(false);

        joinButtons[0].onClick.AddListener(OnJoin0Pressed);
        joinButtons[1].onClick.AddListener(OnJoin1Pressed);
        joinButtons[2].onClick.AddListener(OnJoin2Pressed);
        joinButtons[3].onClick.AddListener(OnJoin3Pressed);

        createPublicLobbyButton.onClick.AddListener(OnCreatePublicLobbyPressed);
        createPrivateLobbyButton.onClick.AddListener(OnCreatePrivateLobbyPressed);
        refreshButton.onClick.AddListener(OnRefreshButtonPressed);
    }


    public void OnRefreshButtonPressed()
    {
        Refresh();
    }

    public void OnJoin0Pressed()
    {
        AddClientToLobbyServerRpc(0, NetworkManager.LocalClientId, passwordInputField.text);
       
    }

    public void OnJoin1Pressed()
    {
        AddClientToLobbyServerRpc(1, NetworkManager.LocalClientId, passwordInputField.text);
        
    }

    public void OnJoin2Pressed()
    {
        AddClientToLobbyServerRpc(2, NetworkManager.LocalClientId, passwordInputField.text);
        
    }

    public void OnJoin3Pressed()
    {
        AddClientToLobbyServerRpc(3, NetworkManager.LocalClientId, passwordInputField.text);
    }

    //METODO QUE UNE A JUGADOR A SALA Y ACTUALIZA INTERFAZ DE TODOS
    [ServerRpc(RequireOwnership = false)]
    public void AddClientToLobbyServerRpc(int lobbyIndex, ulong clientId, string password)
    {
        if (lobbyManager.GetLobbyFromId(lobbyIndex).IsStarted) return;

        if (lobbyManager.GetLobbyFromId(lobbyIndex).IsPrivate)
        {
            Debug.Log("contraseña invalida");
            if (password != lobbyManager.GetLobbyFromId(lobbyIndex).Password) return;
        }


        if (!lobbyManager.AddPlayerToLobby(lobbyIndex, clientId))
            return;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        ChangeToFighterSelectorCLientRpc(clientRpcParams);


        for (int i = 0; i < lobbyManager.Lobbies.Count; i++)
        {
            UpdateUIClientRpc(i, lobbyManager.Lobbies[i].LobbyId, lobbyManager.Lobbies[i].PlayersInLobby);
        }
    }

    //METODO QUE REALIZA LOS PASOS NECESARIOS EN LOS CLIENTES PARA ENTRAR AL SELECTOR DE PERSONAJE
    [ClientRpc]
    public void ChangeToFighterSelectorCLientRpc(ClientRpcParams clientRpcParams = default)
    {
        lobbySelectorUIObject.SetActive(false);
        fighterSelectorUIObject.SetActive(true);
        chatUIObject.SetActive(true);
        GetComponent<ChatUI>().ResetChat();
        GetComponent<FighterSelectorUI>().RefreshServerRpc(NetworkManager.LocalClientId, -1);
    }

    public void OnCreatePublicLobbyPressed()
    {
        CreateLobbyServerRpc(NetworkManager.LocalClientId, false, passwordInputField.text);
        Debug.Log("CLIENTE: quiero crear sala PUBLICA");
    }

    public void OnCreatePrivateLobbyPressed()
    {
        CreateLobbyServerRpc(NetworkManager.LocalClientId, true, passwordInputField.text);
        Debug.Log("CLIENTE: quiero crear sala PRIVADA");
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
    public void CreateLobbyServerRpc(ulong clientId, bool isPrivate, string password)
    {
        Debug.Log("SERVER: creando sala");
        if (!lobbyManager.CreateLobby(clientId, isPrivate, password))
        {
            return;
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };
        ChangeToFighterSelectorCLientRpc(clientRpcParams);
        Debug.Log("SERVER: num de salas actual: " + lobbyManager.Lobbies.Count);

        for (int i = 0; i < lobbyManager.Lobbies.Count; i++)
        {
            UpdateUIClientRpc(i, lobbyManager.Lobbies[i].LobbyId, lobbyManager.Lobbies[i].PlayersInLobby);
        }
    }

    //METODO QUE ACTUALIZA INTERFAZ EN CLIENTE
    [ClientRpc]
    public void UpdateUIClientRpc(int i, int lobbyId, int playersInLobby, ClientRpcParams clientRpcParams = default)
    {

        Debug.Log("CLIENTE: actualizando UI");
        lobbiesUIObjects[i].SetActive(true);
        joinButtonTexts[i].text = $"Lobby {lobbyId + 1}";
        numPlayerstexts[i].text = $"{playersInLobby}/4";
    }
}
