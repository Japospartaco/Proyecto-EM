using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MatchUI : NetworkBehaviour
{
    [Header("UIs utilizadas")]
    [SerializeField] GameObject matchUI;
    [SerializeField] GameObject postMatchUI;
    PostMatchUI postMatchUIScript;

    [Header("Tiempo")]
    [SerializeField] private TMP_Text textBoxTimer;

    [Header("Interfaz de vida")]
    [SerializeField] List<GameObject> playerContainerList;
    [SerializeField] List<Image> playerImageList;
    [SerializeField] List<TMP_Text> playerUsernameList;
    [SerializeField] List<TMP_Text> playerHealthList;

    [Header("Imagenes")]
    [SerializeField] List<Sprite> fighterIcons;

    [Header("Clases auxiliares")]
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
        // Suscribe el evento StartMatch al método InitializeUIHealth
        match.StartMatch += InitializeUIHealth;
    }

    public void SuscribirInterfazVidas(HealthManager healthManager)
    {
        // Suscribe los eventos DmgTakenEvent y ResetHealthEvent del HealthManager a UpdateUIHealth
        healthManager.DmgTakenEvent += UpdateUIHealth;
        healthManager.ResetHealthEvent += UpdateUIHealth;
    }

    public void SuscribirTiempo(CountdownTimer countdownTimer)
    {
        // Suscribe el evento UpdateUITimeEvent del CountdownTimer a UpdateUITimer
        countdownTimer.UpdateUITimeEvent += UpdateUITimer;
    }

    public void SuscribirFinPartida(Match match)
    {
        // Suscribe el evento EndMatchEvent de la partida a UpdateEndUI
        match.EndMatchEvent += UpdateEndUI;
    }

    public void InitializeUIHealth(object sender, Match match)
    {
        // Obtiene la lista de jugadores en el lobby de la partida
        List<PlayerInformation> listPlayers = match.Lobby.PlayersList;

        // Configura los parámetros de envío del RPC a todos los clientes del lobby
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = match.Lobby.GetPlayersIdsList()
            }
        };

        // Itera sobre la lista de jugadores
        for (int i = 0; i < listPlayers.Count; i++)
        {
            PlayerInformation player = listPlayers[i];
            HealthManager healthManager = player.FighterObject.GetComponent<HealthManager>();

            string user = player.Username;
            string health = $"{healthManager.healthPoints}/{healthManager.maxHealth}";
            int selectedFighter = player.SelectedFighter;

            Debug.Log($"SERVIDOR: Inicializando la UI de {user}, con {health} vida y {selectedFighter} personajes seleccionado.");

            // Invoca el RPC para inicializar la UI de salud en los clientes
            InitializeUIHealthClientRpc(i, user, health, selectedFighter, clientRpcParams);
        }
    }

    [ClientRpc]
    void InitializeUIHealthClientRpc(int index, string user, string health, int selectedFighter, ClientRpcParams clientRpcParams = default)
    {
        // Restaura el color original y la imagen en blanco del contenedor del jugador
        playerContainerList[index].GetComponent<Image>().color = originalColorList[index];
        playerImageList[index].color = Color.white;

        // Activa el contenedor del jugador
        playerContainerList[index].SetActive(true);

        // Configura la imagen del jugador con el sprite correspondiente al personaje seleccionado
        playerImageList[index].sprite = fighterIcons[selectedFighter];

        // Establece el nombre de usuario y la salud del jugador en la UI
        playerUsernameList[index].text = user;
        playerHealthList[index].text = health;
    }

    public void UpdateUIHealth(object sender, GameObject fighterDamaged)
    {
        // Verifica si el luchador está desconectado y no actualiza la UI de salud si es así
        if (fighterDamaged.GetComponent<FighterInformation>().IsDisconnected)
        {
            return;
        }

        // Obtiene el HealthManager y la información del jugador del luchador dañado
        HealthManager healthManager = fighterDamaged.GetComponent<HealthManager>();
        PlayerInformation player = fighterDamaged.GetComponent<FighterInformation>().Player.GetComponent<PlayerInformation>();

        // Obtiene el ID del lobby del jugador y el lobby correspondiente
        int lobbyId = lobbyManager.GetPlayersLobby(player.Id);
        Lobby lobby = lobbyManager.GetLobbyFromId(lobbyId);

        // Configura los parámetros de envío del RPC a todos los clientes del lobby
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

        int idInLobby = player.IdInLobby;

        // Invoca el RPC para actualizar la UI de salud del jugador en los clientes
        if (current_health > 0)
            UpdateUIHealthClientRpc(text, idInLobby, clientRpcParams);
        else
            UpdateDeadHealthClientRpc(text, idInLobby, clientRpcParams);
    }

    [ClientRpc]
    public void UpdateUIHealthClientRpc(string text, int idInLobby, ClientRpcParams clientRpcParams = default)
    {
        // Restaura el color original del contenedor del jugador
        playerContainerList[idInLobby].GetComponent<Image>().color = originalColorList[idInLobby];
        playerImageList[idInLobby].color = Color.white;

        // Actualiza el texto de la salud del jugador en la UI
        playerHealthList[idInLobby].text = text;
    }

    [ClientRpc]
    public void UpdateDeadHealthClientRpc(string text, int idInLobby, ClientRpcParams clientRpcParams = default)
    {
        // Cambia el color del contenedor del jugador a un color representativo de la muerte
        playerContainerList[idInLobby].GetComponent<Image>().color = colorDeath;
        playerImageList[idInLobby].color = Color.black;

        // Actualiza el texto de la salud del jugador en la UI
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

        // Verifica si el temporizador ha finalizado
        if (countdownTimer.CurrentTime <= 0)
        {
            text = "00:00";
        }
        else
        {
            // Calcula los minutos y segundos restantes en el temporizador
            int minutes = Mathf.FloorToInt(countdownTimer.CurrentTime / 60);
            int seconds = Mathf.FloorToInt(countdownTimer.CurrentTime % 60);

            // Formatea el texto del temporizador con ceros a la izquierda
            text = $"{minutes:00}:{seconds:00}";
        }

        // Invoca el RPC para actualizar el temporizador en los clientes
        UpdateUITimerClientRpc(text, clientRpcParams);
    }

    [ClientRpc]
    public void UpdateUITimerClientRpc(string text, ClientRpcParams clientRpcParams = default)
    {
        // Actualiza el texto del temporizador en la UI
        textBoxTimer.text = text;
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

        for(int i = 0; i < match.Lobby.PlayersList.Count;i++)
        {
            DesactivateUIContainersClientRpc(i, clientRpcParams);
        }
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
    void DesactivateUIContainersClientRpc(int index, ClientRpcParams clientRpcParams = default)
    {
         playerContainerList[index].SetActive(false);
    }




}
