using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class MatchUI : NetworkBehaviour
{
    [SerializeField] MatchManager matchManager;

    [SerializeField] private TMP_Text textBoxTimer;

    public EventHandler UpdateUITime;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SuscribirTiempo(CountdownTimer countdownTimer)
	{
        if (!IsServer) return;
        Debug.Log("ESTOY EN SUSCRIBIR TIEMPO");
        countdownTimer.UpdateUITime += UpdateUITimer;
	}

    public void UpdateUITimer(object sender, Match match)
	{
        string text;
        Debug.Log("He entrado a updateUiTimer");

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = match.Lobby.GetPlayersIdsList()
            }
        };

        CountdownTimer countdownTimer = match.Playing_Round.Timer;
        float timer = countdownTimer.Timer;
        Debug.Log(timer);
        int minutes = (int)timer / 60;
        int seconds = (int)timer % 60;

        text = setText(minutes, seconds);

        Debug.Log(text);

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
        textBoxTimer.text = $"{text}";
    }







}
