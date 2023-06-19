using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : NetworkBehaviour
{
    [Space]
    [SerializeField] GameObject ChatUIObject;

    [Space]
    [SerializeField] List<TMP_Text> textBoxMensajesList;
    
    [Space]
    [SerializeField] LobbyManager lobbyManager;
    [SerializeField] OnlinePlayers onlinePlayers;

    int MAX_MENSAJES;
    

    // Start is called before the first frame update
    void Start()
    {
        ChatUIObject.SetActive(false);

        MAX_MENSAJES = textBoxMensajesList.Count;
    }


    public void ResetChat()
    {
        foreach(var textBox in textBoxMensajesList)
        {
            textBox.text = "";
        }
    }

    public void ReadStringInClient(string message)
    {
        if (!IsClient)
        {
            Debug.Log("CARACOLES");
            return;
        }
        ulong id = NetworkManager.LocalClientId;
        ReadStringServerRpc(id, message);
        
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReadStringServerRpc(ulong id, string message)
    {

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = lobbyManager.GetLobbyFromId(lobbyManager.GetPlayersLobby(id)).GetPlayersIdsList()
            }
        };

        string username = $"{onlinePlayers.ReturnPlayerInformation(id).Username}";
        AddMessageClientRpc(username, message, clientRpcParams);
    }

    [ClientRpc]
    public void AddMessageClientRpc(string username, string msg, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log(username + ": " + msg);
        string chat = $"{username} : {msg}";

        for (int j = 0; j < MAX_MENSAJES - 1; j++)
        {
            textBoxMensajesList[j].text = textBoxMensajesList[j + 1].text;
        }
        textBoxMensajesList[MAX_MENSAJES - 1].text = chat;
        Debug.Log(chat);
    }

}
