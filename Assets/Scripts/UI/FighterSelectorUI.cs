using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class FighterSelectorUI : NetworkBehaviour
{
    [SerializeField] private GameObject fighterSelectorUIObject;
    [SerializeField] private Button readyButton;
    [SerializeField] private TMP_Dropdown fighterSelectorInput;
    [SerializeField] private List<GameObject> fightersPrefab;
    private OnlinePlayers onlinePlayers;
    private Lobby myLobby;


    private void Start()
    {
        readyButton.onClick.AddListener(OnReadyButtonPressed);
        fighterSelectorUIObject.SetActive(false);
        if (!IsServer) return;
        onlinePlayers = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<OnlinePlayers>();

    }

    //METODO PARA OBTENER EL VALOR DEL DROPDOWN DE SELECCION DE PERSONAJE
    public int GetSelectedFighter()
    {
        return fighterSelectorInput.value;
    }

    // CUANDO EL JUGADOR ESTE LISTO PULSARA ESTE BOTON, Y SE HARA LA GESTION DE JUGADORES LISTOS PARA SPAWNEAR PERSONAJES
    // Y EMPEZAR LA PARTIDA
    public void OnReadyButtonPressed()
    {
        //IMPLEMENTAR EL SISTEMA DE READYS E INICIAR LA PARTIDA

        //CODIGO TEMPORAL QUE SPAWNEA PERSONAJE SEGUN PULSAS EL BOTON
        ulong id = NetworkManager.LocalClientId;
        int selectedFighter = GetSelectedFighter();
        InstantiateCharacterServerRpc(id, selectedFighter);
        fighterSelectorUIObject.SetActive(false);
    }


    [ServerRpc(RequireOwnership = false)]
    public void InstantiateCharacterServerRpc(ulong id, int selectedFighter)
    {
        GameObject characterGameObject = Instantiate(fightersPrefab[selectedFighter]);

        //ASIGNAMOS EL PERSONAJE CREADO AL "PLAYER INFORMATION" DE SU DUEÑO
        onlinePlayers.ReturnPlayerInformation(id).FighterObject = characterGameObject;

        characterGameObject.GetComponent<NetworkObject>().SpawnWithOwnership(id);
        characterGameObject.transform.SetParent(transform, false);
    }
}
