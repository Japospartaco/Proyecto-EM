using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MatchUI : NetworkBehaviour
{
    [Space]
    [SerializeField] GameObject matchUI;
    [SerializeField] GameObject postMatchUI;
    PostMatchUI postMatchUIScript;

    [Space]
    [SerializeField] private TMP_Text textBoxTimer;
    [SerializeField] List<GameObject> playerContainerList;

    [SerializeField] List<Image> playerImageList;
    [SerializeField] List<TMP_Text> playerUsernameList;
    [SerializeField] List<TMP_Text> playerHealthList;

    [Space]
    [SerializeField] List<Sprite> fighterIcons;

    [Space]
    [SerializeField] LobbyManager lobbyManager;

    public EventHandler UpdateUITime;

    private List<Color> originalColorList = new();
    private Vector4 colorDeath = new Vector4(0.0f, 0.0f, 0.0f, 0.5f);

    // Start is called before the first frame update
    void Start()
    {
        matchUI.SetActive(false);
        postMatchUIScript = GetComponent<PostMatchUI>();

        foreach (var playerContainer in playerContainerList)
        {
            originalColorList.Add(playerContainer.GetComponent<Image>().color);
        }
    }

    public void SuscribirInicializarUIHealth(Match match)
    {
        //Debug.Log($"{NetworkManager.LocalClientId}: Intentando suscribir interfaz de inicializar ui de health.");
        match.StartMatch += InitializeUIHealth;
    }

    public void SuscribirInterfazVidas(HealthManager healthManager)
    {
        //Debug.Log($"{NetworkManager.LocalClientId}: Intentando suscribir interfaz de vidas.");
        healthManager.DmgTakenEvent += UpdateUIHealth;
        healthManager.DeadEvent += UpdateUIHealth;
        healthManager.ResetHealthEvent += UpdateUIHealth;
    }

    public void SuscribirTiempo(CountdownTimer countdownTimer)
    {
        //Debug.Log($"{NetworkManager.LocalClientId}: Intentando suscribir interfaz de tiempo.");
        countdownTimer.UpdateUITimeEvent += UpdateUITimer;
    }

    public void SuscribirFinPartida(Match match)
    {
        //Debug.Log($"{NetworkManager.LocalClientId}: Intentando suscribir interfaz de fin de partida.");
        match.EndMatchEvent += UpdateEndUI;
    }

    public void InitializeUIHealth(object sender, Match match)
    {
        // Debug.Log("He llegado a inicializar UI Health"); // Llego aqui
        List<PlayerInformation> listPlayers = match.Lobby.PlayersList;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = match.Lobby.GetPlayersIdsList()
            }
        };

        for (int i = 0; i < listPlayers.Count; i++)
        {
            PlayerInformation player = listPlayers[i];
            HealthManager healthManager = player.FighterObject.GetComponent<HealthManager>();

            string user = player.Username;
            string health = $"{healthManager.healthPoints}/{healthManager.maxHealth}";
            int selectedFighter = player.SelectedFighter;

            InitializeUIHealthClientRpc(i, user, health, selectedFighter, clientRpcParams);
        }

    }

    [ClientRpc]
    void InitializeUIHealthClientRpc(int index, string user, string health, int selectedFighter, ClientRpcParams clientRpcParams = default)
    {
        playerContainerList[index].SetActive(true);
        playerImageList[index].sprite = fighterIcons[selectedFighter];
        playerUsernameList[index].text = user;
        playerHealthList[index].text = health;
    }

    public void UpdateUIHealth(object sender, GameObject fighterDamaged)
    {
        //AQUI SE UPDATEAN LOS VALORES DE LA VIDA.
        if (fighterDamaged.GetComponent<FighterInformation>().IsDisconnected)
        {
            return;
        }

        HealthManager healthManager = fighterDamaged.GetComponent<HealthManager>();
        PlayerInformation player = fighterDamaged.GetComponent<FighterInformation>().Player.GetComponent<PlayerInformation>();

        int lobbyId = lobbyManager.GetPlayersLobby(player.Id);
        Lobby lobby = lobbyManager.GetLobbyFromId(lobbyId);

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = lobby.GetPlayersIdsList()
            }
        };

        int current_health = healthManager.healthPoints;
        int max_health = healthManager.maxHealth;

        string text = $"{current_health}/{max_health}";
        // Debug.Log($"{player.Username}: {text}");

        int idInLobby = player.IdInLobby;

        if (current_health > 0) UpdateUIHealthClientRpc(text, idInLobby, clientRpcParams);
        else UpdateDeadHealthClientRpc(text, idInLobby, clientRpcParams);
    }

    [ClientRpc]
    public void UpdateUIHealthClientRpc(string text, int idInLobby, ClientRpcParams clientRpcParams = default)
    {
        playerContainerList[idInLobby].GetComponent<Image>().color = originalColorList[idInLobby];
        playerImageList[idInLobby].color = Color.white;

        playerHealthList[idInLobby].text = text;
    }

    [ClientRpc]
    public void UpdateDeadHealthClientRpc(string text, int idInLobby, ClientRpcParams clientRpcParams = default)
    {
        playerContainerList[idInLobby].GetComponent<Image>().color = colorDeath;

        playerImageList[idInLobby].color = Color.black;


        playerHealthList[idInLobby].text = text;
    }

    public void UpdateUITimer(object sender, Match match)
    {
        string text;

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
        textBoxTimer.text = $"{text}";
    }

    public void UpdateEndUI(object sender, Match match)
    {
        // Debug.Log("Estoy en UpdateEndUI");

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = match.Lobby.GetPlayersIdsList()
            }
        };

        DesactivateUIContainersClientRpc(match.Lobby.PlayersList.Count);
        UpdateEndUIClientRpc(clientRpcParams);

        postMatchUIScript.ComputeInterfaces(match);
    }

    [ClientRpc]
    void UpdateEndUIClientRpc(ClientRpcParams clientRpcParams = default)
    {
        //Debug.Log("CLIENTE RPC: Cambiando UI partida a UI post partida");
        matchUI.SetActive(false);
        postMatchUI.SetActive(true);
    }

    [ClientRpc]
    void DesactivateUIContainersClientRpc(int n, ClientRpcParams clientRpcParams = default)
    {
        for (int i = 0; i < n; i++)
        {
            playerContainerList[i].SetActive(false);
        }
    }




}
