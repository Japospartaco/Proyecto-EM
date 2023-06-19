using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : NetworkBehaviour
{
    [SerializeField] GameObject ChatUIObject;
    [SerializeField] List<TMP_Text> mensajes;
    [SerializeField] TMP_InputField nameInput;
    [SerializeField] OnlinePlayers onlinePlayers;
    

    // Start is called before the first frame update
    void Start()
    {
        ChatUIObject.SetActive(false);
    }


    [ClientRpc]
    public void AddMessageClientRpc(string msg, string username)
    {
        Debug.Log(username + ": " + msg);
        string chat = $"{username} : {msg}";

            for (int j = 0; j < 3; j++)
            {
                mensajes[j].text = mensajes[j+1].text;
            }
            mensajes[3].text = chat;
            Debug.Log(chat);
        
    }

    public void ReadStringInClient(string s)
    {
        if (!IsClient)
        {
            Debug.Log("CARACOLES");
            return;
        }
        ulong id = NetworkManager.LocalClientId;
        ReadStringServerRpc(id, s);
        
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReadStringServerRpc(ulong id, string s)
    {
        string handler = $"{onlinePlayers.ReturnPlayerInformation(id).Username}";
        AddMessageClientRpc(s, handler);
    }

}
