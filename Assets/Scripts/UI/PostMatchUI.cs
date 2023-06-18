using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PostMatchUI : NetworkBehaviour
{

    [Space]
    [SerializeField] GameObject matchUI;
    [SerializeField] GameObject postMatchUI;
    [SerializeField] GameObject lobbySelectorUI;

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
    [SerializeField] Button buttonVolverAJugar;
    [SerializeField] Button buttonSalirDelJuego;

    [Space]
    [SerializeField] OnlinePlayers onlinePlayers;
    [SerializeField] LobbyManager lobbyManager;
    [SerializeField] MatchManager matchManager;
    //[SerializeField] MatchManager match;

    private LobbySelectorUI lobbySelector;

    // Start is called before the first frame update
    void Start()
    {
        postMatchUI.SetActive(false);

        buttonVolverAJugar.onClick.AddListener(OnButtonPressedVolverAJugar);

        lobbySelector = GetComponent<LobbySelectorUI>();
    }


    // Update is called once per frame
    void Update()
    {

    }
    public void ComputeInterfaces(Match match)
    {
        ShowResult(match);
        matchManager.Destroy(match);
        RemoveAllPlayersFromLobby(match);
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

    public void RemoveAllPlayersFromLobby(Match match)
    {
        Lobby lobby = match.Lobby;
        lobby.RemoveAllPlayers();
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

    public void OnButtonPressedVolverAJugar()
    {
        Debug.Log("CLIENTE: HE PRESIONADO EL BOTON DE VOLVER A JUGAR");
        ulong id = NetworkManager.LocalClientId;
        ComputeOnButtonPressedServerRpc(id);

        lobbySelector.RefreshServerRpc(id);
    }

    [ServerRpc(RequireOwnership = false)]
    void ComputeOnButtonPressedServerRpc(ulong clientId)
    {
        Debug.Log("SERVIDOR: HE PRESIONADO EL BOTON");

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        ReturnSelectorClientRpc(clientRpcParams);
        OcultarUIClientRpc(playersImage.Count, clientRpcParams);
    }

    [ClientRpc]
    public void ReturnSelectorClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("CLIENTE: ME VOY A LA FIGHTER SELECTION");

        postMatchUI.SetActive(false);

        lobbySelectorUI.SetActive(true);

        GetComponent<FighterSelectorUI>().OcultarHiddenObjects();
    }

    [ClientRpc]
    void OcultarUIClientRpc(int countPlayers, ClientRpcParams clientRpcParams = default)
    {
        for(int i = 0; i < countPlayers; i++)
        {
            playersUI[i].SetActive(false);
        }
    }
}