using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PostMatchUI : NetworkBehaviour
{
    // Variables de referencia a los elementos de la interfaz de usuario
    [Header("UIs utilizadas")]
    [SerializeField] GameObject matchUI;
    [SerializeField] GameObject postMatchUI;
    [SerializeField] GameObject lobbySelectorUI;
    [SerializeField] GameObject fighterSelectorUI;
    [SerializeField] GameObject chatUI;

    [Header("Contenedor de los jugadores")]
    [SerializeField] List<GameObject> playersUI;

    [Header("Imagenes")]
    [SerializeField] List<Sprite> fighters_sprite;
    [SerializeField] List<Image> playersImage;
    [SerializeField] Image winnerImage;

    [Header("Cajas de texto")]
    [SerializeField] List<TMP_Text> playersInformation;
    [SerializeField] TMP_Text winnerInformation;

    [Header("Botones")]
    [SerializeField] Button buttonReturnLobbySelector;
    [SerializeField] Button buttonReturnFighterSelector;

    [Header("Clases auxiliares")]
    [SerializeField] OnlinePlayers onlinePlayers;
    [SerializeField] LobbyManager lobbyManager;
    [SerializeField] MatchManager matchManager;

    int clientLobbyId = -1;
    //[SerializeField] MatchManager match;

    private LobbySelectorUI lobbySelector;

    // Start is called before the first frame update
    void Start()
    {
        postMatchUI.SetActive(false);

        buttonReturnLobbySelector.onClick.AddListener(OnButtonReturnLobbySelectorPressed);
        buttonReturnFighterSelector.onClick.AddListener(OnButtonReturnFighterSelectorPressed);

        lobbySelector = GetComponent<LobbySelectorUI>();
    }

    // Método que se llama para mostrar la interfaz de usuario posterior a una partida
    public void ComputeInterfaces(Match match)
    {
        int lobbyId = match.Lobby.LobbyId;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = match.Lobby.GetPlayersIdsList()
            }
        };

        SelectLobbyIDClientRpc(lobbyId, clientRpcParams);

        ShowResult(match);
        matchManager.Destroy(match);
        RemoveAllPlayersFromLobby(match);
    }

    // Método RPC del cliente que establece el ID del lobby
    [ClientRpc]
    void SelectLobbyIDClientRpc(int lobbyId, ClientRpcParams clientRpcParams = default)
    {
        clientLobbyId = lobbyId;
        Debug.Log($"{NetworkManager.LocalClientId}: lobby id seteado.");
    }

    // Muestra los resultados de la partida en la interfaz de usuario
    void ShowResult(Match match)
    {
        Debug.Log("Soy servidor");
        List<PlayerInformation> jugadores = match.Players;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = match.Lobby.GetPlayersIdsList()
            }
        };

        PlayerInformation winner = match.Player_Winner;
        string winner_text = $"{winner.Username}\n-----{winner.FighterObject.GetComponent<FighterInformation>().WinnedRounds}-----";

        for (int i = 0; i < jugadores.Count; i++)
        {
            GameObject fighter = jugadores[i].FighterObject;

            string text;
            text = $"{jugadores[i].Username}\n-----{fighter.GetComponent<FighterInformation>().WinnedRounds}-----";

            MostrarInterfazJugadoresClientRpc(i, jugadores[i].SelectedFighter, text, winner.SelectedFighter, winner_text, clientRpcParams);
        }

        MostrarInterfazGanadorClientRpc(winner.SelectedFighter, winner_text, clientRpcParams);
    }

    // Método RPC del cliente para mostrar la interfaz de usuario de los jugadores
    [ClientRpc]
    void MostrarInterfazJugadoresClientRpc(int index, int selectedFighter, string text, int winnerSelectedFighter, string textWinner, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("Mostrando interfaz final");

        playersUI[index].SetActive(true);
        playersImage[index].sprite = fighters_sprite[selectedFighter];
        playersInformation[index].text = text;
    }

    // Método RPC del cliente para mostrar la interfaz de usuario del ganador
    [ClientRpc]
    void MostrarInterfazGanadorClientRpc(int winnerSelectedFighter, string textWinner, ClientRpcParams clientRpcParams = default)
    {
        winnerImage.sprite = fighters_sprite[winnerSelectedFighter];
        winnerInformation.text = textWinner;
    }

    // Elimina a todos los jugadores del lobby
    public void RemoveAllPlayersFromLobby(Match match)
    {
        Lobby lobby = match.Lobby;
        lobby.RemoveAllPlayers();
    }

    // Maneja el evento de presionar el botón de volver al lobby selector
    public void OnButtonReturnLobbySelectorPressed()
    {
        Debug.Log("CLIENTE: HE PRESIONADO EL BOTON DE VOLVER AL LOBBY SELECTOR");
        ulong id = NetworkManager.LocalClientId;
        ComputeOnButtonReturnLobbySelectorPressedServerRpc(id);

        lobbySelector.RefreshServerRpc(id);
    }

    // Método RPC del servidor para manejar el evento de presionar el botón de volver al lobby selector
    [ServerRpc(RequireOwnership = false)]
    void ComputeOnButtonReturnLobbySelectorPressedServerRpc(ulong clientId)
    {
        Debug.Log("SERVIDOR: HE PRESIONADO EL BOTON DE VOLVER AL LOBBY SELECTOR");

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        ReturnToLobbySelectorClientRpc(clientRpcParams);
        OcultarUIClientRpc(playersImage.Count, clientRpcParams);
    }

    // Método RPC del cliente para volver al lobby selector
    [ClientRpc]
    public void ReturnToLobbySelectorClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("CLIENTE: ME VOY A LA LOBBY SELECTOR");

        postMatchUI.SetActive(false);

        lobbySelectorUI.SetActive(true);
    }

    // Maneja el evento de presionar el botón de volver al selector de luchadores
    void OnButtonReturnFighterSelectorPressed()
    {
        Debug.Log("CLIENTE: HE PRESIONADO EL BOTON DE VOLVER AL LOBBY SELECTOR");

        ComputeOnButtonReturnFighterSelectorPressedServerRpc(NetworkManager.LocalClientId, clientLobbyId);
    }

    // Método RPC del servidor para manejar el evento de presionar el botón de volver al selector de luchadores
    [ServerRpc(RequireOwnership = false)]
    private void ComputeOnButtonReturnFighterSelectorPressedServerRpc(ulong clientId, int clientLobbyId)
    {
        Debug.Log("SERVIDOR: HE PRESIONADO EL BOTON DE VOLVER AL LOBBY SELECTOR");

        if (clientLobbyId == -1)
        {
            Debug.Log("SERVIDOR: Lobby no encontrada.");
            return;
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        lobbyManager.AddPlayerToLobby(clientLobbyId, clientId);

        ReturnToFighterSelectorClientRpc(clientRpcParams);
        OcultarUIClientRpc(playersImage.Count, clientRpcParams);
    }

    // Método RPC del cliente para volver al selector de luchadores
    [ClientRpc]
    private void ReturnToFighterSelectorClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("CLIENTE: ME VOY A LA FIGHTER SELECTION");

        postMatchUI.SetActive(false);
        GetComponent<FighterSelectorUI>().RefreshServerRpc(NetworkManager.LocalClientId, -1);

        fighterSelectorUI.SetActive(true);
        chatUI.SetActive(true);
        GetComponent<ChatUI>().ResetChat();
    }

    // Método RPC del cliente para ocultar la interfaz de usuario de los jugadores
    [ClientRpc]
    void OcultarUIClientRpc(int countPlayers, ClientRpcParams clientRpcParams = default)
    {
        for (int i = 0; i < countPlayers; i++)
        {
            playersUI[i].SetActive(false);
        }
    }
}
