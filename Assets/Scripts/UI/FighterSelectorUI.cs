using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;



public class FighterSelectorUI : NetworkBehaviour
{
    [SerializeField] private List<TMP_Text> playersText;

    [Space][SerializeField] private GameObject fighterSelectorUIObject;
    [SerializeField] private GameObject lobbySelectorUIObject;
    [SerializeField] private GameObject matchUIObject;
    [SerializeField] private GameObject chatSelectorUIObject;

    [Space][SerializeField] private Button readyButton;
    [SerializeField] private Button returnButton;

    [Space][SerializeField] private TMP_Dropdown fighterSelectorInput;
    [SerializeField] private List<GameObject> fightersPrefab;

    [Space] [SerializeField] private MatchManager matchManager;
    [SerializeField] private TMP_Dropdown roundNumberSelectorInput;
    [SerializeField] private int[] roundNumberOptions;

    [SerializeField] private TMP_Dropdown timeSelectorInput;
    [SerializeField] private int[] timeOptions;


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

    //METODO PARA OBTENER EL VALOR DEL DROPDOWN DE SELECCION DE PERSONAJE
    public int GetSelectedFighter()
    {
        return fighterSelectorInput.value;
    }

    public void OnReturnButtonPressed()
    {
        ulong clientId = NetworkManager.LocalClientId;
        ReturnFromLobbyServerRpc(clientId);

        lobbySelectorUIObject.SetActive(true);

        timeSelectorInput.gameObject.SetActive(false);
        roundNumberSelectorInput.gameObject.SetActive(false);

        foreach(var text in playersText)
        {
            text.text = "NONE";
        }

        fighterSelectorUIObject.SetActive(false);
        chatSelectorUIObject.SetActive(false);
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
        PlayerReadyServerRpc(NetworkManager.LocalClientId, GetSelectedFighter());
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerReadyServerRpc(ulong playerId, int selectedFighter)
    {
        onlinePlayers.ReturnPlayerInformation(playerId).SelectedFighter = selectedFighter;

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

        foreach (var player in lobby.PlayersList)
        {
            InstantiateCharacter(player.Id, player.SelectedFighter);
        }
        int n_rounds = lobby.RoundNumber;
        int time_per_round = lobby.RoundTime;

        Debug.Log("Quiero empezar la partida");
        Debug.Log($"NUMERO DE RONDAS: {n_rounds}");
        Debug.Log($"TIEMPO POR RONDA: {time_per_round}");

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = lobby.GetPlayersIdsList()
            }
        };

        StartGameClientRpc(clientRpcParams);
        
        matchManager.AddMatch(new Match(lobby, n_rounds, time_per_round, matchManager));
    }


    public void InstantiateCharacter(ulong id, int selectedFighter)
    {
        if (!IsServer) return;
        GameObject characterGameObject = Instantiate(fightersPrefab[selectedFighter]);

        //ASIGNAMOS EL PERSONAJE CREADO AL "PLAYER INFORMATION" DE SU DUE�O
        GameObject player = onlinePlayers.ReturnPlayerGameObject(id);
        player.GetComponent<PlayerInformation>().FighterObject = characterGameObject;
        characterGameObject.GetComponent<FighterInformation>().Player = player;

        matchManager.AddEventHealthInterface(characterGameObject.GetComponent<HealthManager>());

        characterGameObject.GetComponent<NetworkObject>().SpawnWithOwnership(id);
        characterGameObject.transform.SetParent(transform, false);
    }

    [ClientRpc]
    public void StartGameClientRpc(ClientRpcParams clientRpcParams = default)
    {
        OcultarRoundTimeOptions();
        fighterSelectorUIObject.SetActive(false);
        chatSelectorUIObject.SetActive(false);
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
        TimeChangedServerRpc(NetworkManager.LocalClientId ,timeOptions[value]);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TimeChangedServerRpc(ulong clientId, int time)
    {
        int lobbyId = lobbyManager.GetPlayersLobby(clientId);
        Lobby lobby = lobbyManager.GetLobbyFromId(lobbyId);

        lobby.RoundTime = time;
        Debug.Log("SERVER ACTUALIZA TIEMPO: " +lobby.RoundTime);
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
}
