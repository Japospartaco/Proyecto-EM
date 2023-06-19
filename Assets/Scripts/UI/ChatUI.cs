using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : NetworkBehaviour
{
    public TMPro.TMP_Text[] mensaje = new TMPro.TMP_Text[4];
    public int i = 0;
    public string input;
    public TMP_InputField nameInput;
    private OnlinePlayers onlinePlayers;
    // Start is called before the first frame update
    void Start()
    {
        onlinePlayers = FindObjectOfType<OnlinePlayers>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ResetChat()
    {

        for (int j = 0; j < 4; j++)
        {
            mensaje[j].text = "";
        }
    }

    [ClientRpc]
    public void AddMessageClientRpc(string msg, string username)
    {
        Debug.Log(username + ": " + msg);
        string chat = $"{username} : {msg}";

            for (int j = 0; j < 3; j++)
            {
                mensaje[j].text = mensaje[j+1].text;
            }
            mensaje[3].text = chat;
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
