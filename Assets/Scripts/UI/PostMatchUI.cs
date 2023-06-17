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
    [SerializeField] GameObject fighterSelectorUI;

    [Space]
    [SerializeField] List<GameObject> playersUI;
    [SerializeField] GameObject winnerUI;

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
    [SerializeField] MatchManager matchManager;


    public MatchManager MatchPlayed { get { return matchManager; } set { matchManager = value; } }


    // Start is called before the first frame update
    void Start()
    {
        postMatchUI.SetActive(false);

    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowResult(Match match)
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
    }

    [ClientRpc]
    void MostrarInterfazJugadoresClientRpc(int index, int selectedFighter, string text, int winnerSelectedFighter, string textWinner ,ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("Mostrando interfaz final");

        playersUI[index].SetActive(true);
        playersImage[index].sprite = fighters_sprite[selectedFighter];
        playersInformation[index].text = text;

        winnerImage.sprite = fighters_sprite[winnerSelectedFighter];
        winnerInformation.text = textWinner;
    }

    public void OnButtonPressedVolverAJugar()
    {

    }
}
