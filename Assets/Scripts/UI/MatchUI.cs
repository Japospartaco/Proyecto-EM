using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class MatchUI : NetworkBehaviour
{
    [SerializeField] GameObject matchUI;
    [SerializeField] GameObject postMatchUI;
    PostMatchUI postMatchUIScript;

    [SerializeField] MatchManager matchManager;

    [SerializeField] private TMP_Text textBoxTimer;

    public EventHandler UpdateUITime;

    // Start is called before the first frame update
    void Start()
    {
        matchUI.SetActive(false);
        postMatchUIScript = GameObject.FindGameObjectWithTag("UI Manager").GetComponent<PostMatchUI>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SuscribirTiempo(CountdownTimer countdownTimer)
	{
        countdownTimer.UpdateUITime += UpdateUITimer;
	}

    public void SuscribirFinPartida(Match match)
    {
        match.EndMatchEvent += UpdateEndUI;
    }

    public void UpdateUITimer(object sender, Match match)
	{
        string text;
        Debug.Log("Estoy en UpdateUITimer");

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = match.Lobby.GetPlayersIdsList()
            }
        };

        CountdownTimer countdownTimer = match.Playing_Round.Timer;
        float timer = countdownTimer.Timer;

        int minutes = (int)timer / 60;
        int seconds = (int)timer % 60;

        text = setText(minutes, seconds);

        ActualizarTiempoClientRpc(text, clientRpcParams);
    }

    string setText(int minutes, int seconds)
	{
        string text;
        string minutes_string;
        string seconds_string;

        if (minutes < 10)
            minutes_string = $"0{minutes}";
        else
            minutes_string = $"{minutes}";

        if (seconds < 10)
            seconds_string = $"0{seconds}";
        else
            seconds_string = $"{seconds}";

        text = $"{minutes_string}:{seconds_string}";

        return text;
    }


    [ClientRpc]
    private void ActualizarTiempoClientRpc(string text, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("CLIENTE RPC: Estoy en actualizar tiempo cliente rpc");
        textBoxTimer.text = $"{text}";
    }

    public void UpdateEndUI(object sender, Match match)
    {
        Debug.Log("Estoy en UpdateEndUI");

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = match.Lobby.GetPlayersIdsList()
            }
        };

        UpdateEndUIClientRpc(clientRpcParams);

        postMatchUIScript.ComputeInterfaces(match);
    }

    [ClientRpc]
    void UpdateEndUIClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("CLIENTE RPC: Cambiando UI partida a UI post partida");
        matchUI.SetActive(false);
        postMatchUI.SetActive(true);
    }




}
