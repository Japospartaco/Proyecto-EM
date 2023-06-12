using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;



public class FighterSelectorUI : NetworkBehaviour
{
    [SerializeField] private List<GameObject> beforeRefreshHiddenObjects;

    [SerializeField] private List<TMP_Text> playersText;
    [SerializeField] private GameObject fighterSelectorUIObject;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private TMP_Dropdown fighterSelectorInput;
    [SerializeField] private List<GameObject> fightersPrefab;
    private OnlinePlayers onlinePlayers;
    private LobbyManager lobbyManager;

    private List<NetworkVariable<string>> playerStrings;

    private void Start()
    {
        fighterSelectorUIObject.SetActive(false);
        readyButton.onClick.AddListener(OnReadyButtonPressed);
        refreshButton.onClick.AddListener(OnRefreshButtonPressed);

        onlinePlayers = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<OnlinePlayers>();
        lobbyManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<LobbyManager>();

    }

    //METODO PARA OBTENER EL VALOR DEL DROPDOWN DE SELECCION DE PERSONAJE
    public int GetSelectedFighter()
    {
        return fighterSelectorInput.value;
    }


    //METODO PARA ACTUALIZAR INTERFAZ
    public void OnRefreshButtonPressed()
    {
        RefreshServerRpc(NetworkManager.LocalClientId);
        foreach(var obj in beforeRefreshHiddenObjects)
        {
            obj.SetActive(true);
        }
        refreshButton.gameObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RefreshServerRpc(ulong clientId)
    {
        int lobbyId = lobbyManager.GetPlayersLobby(clientId);
        Lobby myLobby = lobbyManager.Lobbies[lobbyId];

        for (int i = 0; i < myLobby.PlayersInLobby; i++)
        {
            ulong id = myLobby.PlayersList[i].Id;
            string text = "";
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
    }

    // CUANDO EL JUGADOR ESTE LISTO PULSARA ESTE BOTON, Y SE HARA LA GESTION DE JUGADORES LISTOS PARA SPAWNEAR PERSONAJES Y EMPEZAR LA PARTIDA
    public void OnReadyButtonPressed()
    {
        //IMPLEMENTAR EL SISTEMA DE READYS E INICIAR LA PARTIDA
        PlayerReadyServerRpc(NetworkManager.LocalClientId, GetSelectedFighter());

        //CODIGO TEMPORAL QUE SPAWNEA PERSONAJE SEGUN PULSAS EL BOTON
        ulong id = NetworkManager.LocalClientId;
        int selectedFighter = GetSelectedFighter();
        InstantiateCharacterServerRpc(id, selectedFighter);
        fighterSelectorUIObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerReadyServerRpc(ulong playerId, int selectedFighter)
    {
        onlinePlayers.ReturnPlayerInformation(playerId).SelectedFighter = selectedFighter;

        Lobby lobby = lobbyManager.Lobbies[lobbyManager.GetPlayersLobby(playerId)];
        lobby.PlayerReady(playerId);

        if (lobby.ReadyPlayers.Count >= lobby.PlayersInLobby / 2)

            //#################################### AQUI COMIENZA LA PARTIDA ##################################################
            StartGame(lobby);
    }


    //####### METODO QUE COMIENZA LA PARTIDA ############
    private void StartGame(Lobby lobby)
    {
        foreach (var player in lobby.PlayersList)
        {
            InstantiateCharacterServerRpc(player.Id, player.SelectedFighter);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void InstantiateCharacterServerRpc(ulong id, int selectedFighter)
    {
        GameObject characterGameObject = Instantiate(fightersPrefab[selectedFighter]);

        //ASIGNAMOS EL PERSONAJE CREADO AL "PLAYER INFORMATION" DE SU DUEÑO
        onlinePlayers.ReturnPlayerInformation(id).FighterObject = characterGameObject;

        characterGameObject.GetComponent<NetworkObject>().SpawnWithOwnership(id);
        characterGameObject.transform.SetParent(transform, false);
    }
}
