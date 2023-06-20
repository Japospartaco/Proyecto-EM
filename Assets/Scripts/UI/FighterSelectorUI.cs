using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Movement.Components;

public class FighterSelectorUI : NetworkBehaviour
{
    [SerializeField] private List<TMP_Text> playersText;

    [Space][SerializeField] private GameObject fighterSelectorUIObject;
    [SerializeField] private GameObject chatUIObject;
    [SerializeField] private GameObject lobbySelectorUIObject;
    [SerializeField] private GameObject matchUIObject;

    [Space][SerializeField] private Button readyButton;
    [SerializeField] private Button returnButton;

    [Space][SerializeField] private TMP_Dropdown fighterSelectorInput;
    [SerializeField] private List<GameObject> fightersPrefab;

    [Space][SerializeField] private MatchManager matchManager;
    [SerializeField] private TMP_Dropdown roundNumberSelectorInput;
    [SerializeField] private int[] roundNumberOptions;

    [SerializeField] private TMP_Dropdown timeSelectorInput;
    [SerializeField] private int[] timeOptions;

    [Space]
    [SerializeField] List<LayerMask> layerLobbies;

    

    private OnlinePlayers onlinePlayers;
    private LobbyManager lobbyManager;

    private List<NetworkVariable<string>> playerStrings;

    private void Start()
    {
        fighterSelectorUIObject.SetActive(false);
        
        readyButton.onClick.AddListener(OnReadyButtonPressed);
        returnButton.onClick.AddListener(OnReturnButtonPressed);

        //timeSelectorInput.onValueChanged.AddListener(OnTimeChanged);
        //roundNumberSelectorInput.onValueChanged.AddListener(OnRoundsChanged);

        onlinePlayers = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<OnlinePlayers>();
        lobbyManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<LobbyManager>();

    }


    public void OnReturnButtonPressed()
    {
        ulong clientId = NetworkManager.LocalClientId;
        ReturnFromLobbyServerRpc(clientId);

        lobbySelectorUIObject.SetActive(true);

        timeSelectorInput.gameObject.SetActive(false);
        roundNumberSelectorInput.gameObject.SetActive(false);

        foreach (var text in playersText)
        {
            text.text = "NONE";
        }

        fighterSelectorUIObject.SetActive(false);
        chatUIObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReturnFromLobbyServerRpc(ulong clientId)
    {
        int lobbyId = lobbyManager.GetPlayersLobby(clientId);
        Lobby lobby = lobbyManager.GetLobbyFromId(lobbyId);

        lobby.RemovePlayerFromLobby(clientId);
        RefreshServerRpc(clientId, lobbyId);
    }


    //ACTUALIZA LA INTERFAZ DE LOS MIEMBROS DE UNA SALA  --- SI LE PASAS UN LOBBY CONCRETO LO HACE DE ESE LOBBY, SI NO, EL DEL USUARIO QUE LO ENVIA
    [ServerRpc(RequireOwnership = false)]
    public void RefreshServerRpc(ulong clientId, int playerLobbyId)
    {

        //solo el jugador 1 puede modificar las opciones de rondas y tiempo
        if (onlinePlayers.ReturnPlayerInformation(clientId).IdInLobby == 0)
        {
            Debug.Log("Hola, voy a hacer refresh server rpc");
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { clientId }
                }
            };
            Player1RoundTimeOptionsClientRpc(clientRpcParams);
        }


        int lobbyId;
        if (playerLobbyId != -1)
        {
            lobbyId = playerLobbyId;
        }
        else
        {
            lobbyId = lobbyManager.GetPlayersLobby(clientId);
        }

        Lobby myLobby = lobbyManager.GetLobbyFromId(lobbyId);

        for (int i = 0; i < myLobby.PlayersInLobby; i++)
        {
            ulong id = myLobby.PlayersList[i].Id;
            string text;

            if (myLobby.IsPlayerReady(id))
            {
                text = $"{onlinePlayers.ReturnPlayerInformation(id).Username}\n" +
                    $"--READY--";
            }
            else
            {
                text = $"{onlinePlayers.ReturnPlayerInformation(id).Username}\n" +
                    $"--NOT READY--";
            }
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = myLobby.GetPlayersIdsList()
                }
            };
            RefreshClientRpc(i, text, clientRpcParams);
        }
    }

    //METODO QUE ACTUALIZA INTERFAZ
    [ClientRpc]
    public void RefreshClientRpc(int i, string text, ClientRpcParams clientRpcParams = default)
    {
        playersText[i].text = text;

        for (int j = i + 1; j < 4; j++)
        {
            playersText[j].text = "NONE";
        }
    }

    [ClientRpc]
    public void Player1RoundTimeOptionsClientRpc(ClientRpcParams clientRpcParams = default)
    {
        timeSelectorInput.gameObject.SetActive(true);
        roundNumberSelectorInput.gameObject.SetActive(true);
    }



    // CUANDO EL JUGADOR ESTE LISTO PULSARA ESTE BOTON, Y SE HARA LA GESTION DE JUGADORES LISTOS PARA SPAWNEAR PERSONAJES Y EMPEZAR LA PARTIDA
    public void OnReadyButtonPressed()
    {
        //IMPLEMENTAR EL SISTEMA DE READYS E INICIAR LA PARTIDA
        PlayerReadyServerRpc(NetworkManager.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerReadyServerRpc(ulong playerId)
    {
        Lobby lobby = lobbyManager.GetLobbyFromId(lobbyManager.GetPlayersLobby(playerId));
        lobby.PlayerReady(playerId);

        Debug.Log($"HAY {lobby.NumberOfReadyPlayers} JUGADORES LISTOS");

        RefreshServerRpc(playerId, -1);

        if (lobby.PlayersInLobby == 1) return;

        if (lobby.NumberOfReadyPlayers >= (lobby.PlayersInLobby / 2) + 1)
        {
            lobby.IsStarted = true;
            //#################################### AQUI COMIENZA LA PARTIDA ##################################################
            StartGame(lobby);
        }
    }


    //####### METODO QUE COMIENZA LA PARTIDA ############
    private void StartGame(Lobby lobby)
    {
        if (!IsServer) return;

        GameObject initPos = GameObject.FindGameObjectWithTag("Spawn positions");
        List<Transform> transformIniciales = new List<Transform>();

        for (int i = 0; i < initPos.transform.childCount; i++)
        {
            transformIniciales.Add(initPos.transform.GetChild(i));
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = lobby.GetPlayersIdsList()
            }
        };


        int n_rounds = lobby.RoundNumber;
        int time_per_round = lobby.RoundTime;

        Debug.Log("Quiero empezar la partida");
        Debug.Log($"NUMERO DE RONDAS: {n_rounds}");
        Debug.Log($"TIEMPO POR RONDA: {time_per_round}");

        StartGameClientRpc(clientRpcParams);

        for (int i = 0; i < lobby.PlayersList.Count; i++)
        {
            PlayerInformation player = lobby.PlayersList[i];
            InstantiateCharacter(lobby, player.Id, player.SelectedFighter, transformIniciales[i], clientRpcParams);

        }

        matchManager.AddMatch(new Match(lobby, n_rounds, time_per_round, matchManager, transformIniciales));
    }


    public void InstantiateCharacter(Lobby lobby, ulong id, int selectedFighter, Transform posInit, ClientRpcParams clientRpcParams)
    {
        if (!IsServer) return;
        GameObject characterGameObject = Instantiate(fightersPrefab[selectedFighter], posInit);

        //ASIGNAMOS EL PERSONAJE CREADO AL "PLAYER INFORMATION" DE SU DUE�O
        GameObject player = onlinePlayers.ReturnPlayerGameObject(id);
        player.GetComponent<PlayerInformation>().FighterObject = characterGameObject;
        characterGameObject.GetComponent<FighterInformation>().Player = player;

        GameObject lobbiesLayersGameObject = GameObject.FindGameObjectWithTag("Lobbies");
        AssignLayerRecursively(characterGameObject, lobbiesLayersGameObject.transform.GetChild(lobby.LobbyId).gameObject.layer);

        /*
        switch (lobby.LobbyId)
        {
            case 0:
                AssignLayerRecursively(characterGameObject, lobbiesLayersGameObject.transform.GetChild(0).gameObject.layer);
                break;
            case 1:
                AssignLayerRecursively(characterGameObject, lobbiesLayersGameObject.transform.GetChild(1).gameObject.layer);
                break;
            case 2:
                AssignLayerRecursively(characterGameObject, lobbiesLayersGameObject.transform.GetChild(2).gameObject.layer);
                break;
            case 3:
                AssignLayerRecursively(characterGameObject, lobbiesLayersGameObject.transform.GetChild(3).gameObject.layer);
                break;
        }*/

        matchManager.AddEventHealthInterface(characterGameObject.GetComponent<HealthManager>());

        NetworkObject networkObject = characterGameObject.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(id);
        networkObject.DontDestroyWithOwner = true;

        foreach (var clientId in onlinePlayers.OnlinePlayersDictionary.Keys)
        {
            networkObject.NetworkHide(clientId);
        }

        characterGameObject.transform.SetParent(transform, false);
    }

    public void AssignLayerRecursively(GameObject gameObject, int layer)
    {
        // Asignar la capa al objeto actual
        gameObject.layer = layer;

        // Recorrer todos los hijos del objeto actual y asignar la capa de manera recursiva
        foreach (Transform child in gameObject.transform)
        {
            AssignLayerRecursively(child.gameObject, layer);
        }
    }


    [ClientRpc]
    public void StartGameClientRpc(ClientRpcParams clientRpcParams = default)
    {
        OcultarRoundTimeOptions();
        fighterSelectorUIObject.SetActive(false);
        chatUIObject.SetActive(false);
        matchUIObject.SetActive(true);
    }

    public void OcultarRoundTimeOptions()
    {
        timeSelectorInput.gameObject.SetActive(false);
        roundNumberSelectorInput.gameObject.SetActive(false);
    }

    public void OnTimeChanged(int value)
    {
        Debug.Log("CLIENTE CAMBIA TIEMPO: " + value);
        TimeChangedServerRpc(NetworkManager.LocalClientId, timeOptions[value]);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TimeChangedServerRpc(ulong clientId, int time)
    {
        int lobbyId = lobbyManager.GetPlayersLobby(clientId);
        Lobby lobby = lobbyManager.GetLobbyFromId(lobbyId);

        lobby.RoundTime = time;
        Debug.Log("SERVER ACTUALIZA TIEMPO: " + lobby.RoundTime);
    }

    public void OnRoundsChanged(int value)
    {
        Debug.Log("CLIENTE CAMBIA RONDA: " + value);
        RoundsChangedServerRpc(NetworkManager.LocalClientId, roundNumberOptions[value]);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RoundsChangedServerRpc(ulong clientId, int rounds)
    {
        Debug.Log("SERVER INTENTA ACTUALIZAR RONDAS");
        int lobbyId = lobbyManager.GetPlayersLobby(clientId);
        Lobby lobby = lobbyManager.GetLobbyFromId(lobbyId);

        lobby.RoundNumber = rounds;
        Debug.Log("SERVER ACTUALIZA RONDAS: " + lobby.RoundNumber);
    }

    public void OnCharacterChanged(int value)
    { 
        Debug.Log("CLIENTE CAMBIA Personaje: " + value);
        CharacterChangedServerRpc(NetworkManager.LocalClientId, value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void CharacterChangedServerRpc(ulong clientId, int selectedFighter)
    {
        onlinePlayers.ReturnPlayerInformation(clientId).SelectedFighter = selectedFighter;
        
    }
}
