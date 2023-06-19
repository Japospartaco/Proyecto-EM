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

    [Space]
    [SerializeField] GameObject matchUI;
    [SerializeField] GameObject postMatchUI;
    [SerializeField] GameObject lobbySelectorUI;
    [SerializeField] GameObject fighterSelectorUI;
    [SerializeField] GameObject chatUI;

    [Space]
    [SerializeField] List<GameObject> playersUI;

    [Space]
    [SerializeField] List<Sprite> fighters_sprite;
    [SerializeField] List<Image> playersImage;
    [SerializeField] Image winnerImage;

    [Space]
    [SerializeField] List<TMP_Text> playersInformation;
    [SerializeField] TMP_Text winnerInformation;

    [Space]
    [SerializeField] Button buttonReturnLobbySelector;
    [SerializeField] Button buttonReturnFighterSelector;

    [Space]
    [SerializeField] OnlinePlayers onlinePlayers;
    [SerializeField] LobbyManager lobbyManager;
    [SerializeField] MatchManager matchManager;

    int clientLobbyId;
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

    [ClientRpc]
    void SelectLobbyIDClientRpc(int lobbyId, ClientRpcParams clientRpcParams = default)
    {
        clientLobbyId = lobbyId;
        Debug.Log($"{NetworkManager.LocalClientId}: lobby id seteado.");
    }

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


    [ClientRpc]
    void MostrarInterfazJugadoresClientRpc(int index, int selectedFighter, string text, int winnerSelectedFighter, string textWinner, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("Mostrando interfaz final");

        playersUI[index].SetActive(true);
        playersImage[index].sprite = fighters_sprite[selectedFighter];
        playersInformation[index].text = text; 
    }

    [ClientRpc]
    void MostrarInterfazGanadorClientRpc(int winnerSelectedFighter, string textWinner, ClientRpcParams clientRpcParams = default)
    {
        winnerImage.sprite = fighters_sprite[winnerSelectedFighter];
        winnerInformation.text = textWinner;
    }

    public void RemoveAllPlayersFromLobby(Match match)
    {
        Lobby lobby = match.Lobby;
        lobby.RemoveAllPlayers();
    }

    public void OnButtonReturnLobbySelectorPressed()
    {
        Debug.Log("CLIENTE: HE PRESIONADO EL BOTON DE VOLVER AL LOBBY SELECTOR");
        ulong id = NetworkManager.LocalClientId;
        ComputeOnButtonReturnLobbySelectorPressedServerRpc(id);

        lobbySelector.RefreshServerRpc(id);
    }

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

    [ClientRpc]
    public void ReturnToLobbySelectorClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("CLIENTE: ME VOY A LA LOBBY SELECTOR");

        postMatchUI.SetActive(false);

        lobbySelectorUI.SetActive(true);

       // GetComponent<FighterSelectorUI>().OcultarHiddenObjects();
    }



    void OnButtonReturnFighterSelectorPressed()
    {
        Debug.Log("CLIENTE: HE PRESIONADO EL BOTON DE VOLVER AL LOBBY SELECTOR");

        ComputeOnButtonReturnFighterSelectorPressedServerRpc(NetworkManager.LocalClientId, clientLobbyId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ComputeOnButtonReturnFighterSelectorPressedServerRpc(ulong clientId, int clientLobbyId)
    {
        Debug.Log("SERVIDOR: HE PRESIONADO EL BOTON DE VOLVER AL LOBBY SELECTOR");

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

    [ClientRpc]
    void OcultarUIClientRpc(int countPlayers, ClientRpcParams clientRpcParams = default)
    {
        for (int i = 0; i < countPlayers; i++)
        {
            playersUI[i].SetActive(false);
        }
    }
}