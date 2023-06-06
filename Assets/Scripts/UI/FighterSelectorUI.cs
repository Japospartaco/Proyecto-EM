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


    private void Start()
    {
        readyButton.onClick.AddListener(OnReadyButtonPressed);
        fighterSelectorUIObject.SetActive(false);
    }
    public int GetSelectedFighter()
    {
        return fighterSelectorInput.value;
    }

    public void OnReadyButtonPressed()
    {
        //IMPLEMENTAR EL SISTEMA DE READYS E INICIAR LA PARTIDA
        ulong id = NetworkManager.LocalClientId;
        int selectedFighter = GetSelectedFighter();
        InstantiateCharacterServerRpc(id, selectedFighter);
        fighterSelectorUIObject.SetActive(false);
    }


    [ServerRpc(RequireOwnership = false)]
    public void InstantiateCharacterServerRpc(ulong id, int selectedFighter)
    {
        GameObject characterGameObject = Instantiate(fightersPrefab[selectedFighter]);
        characterGameObject.GetComponent<NetworkObject>().SpawnWithOwnership(id);
        characterGameObject.transform.SetParent(transform, false);
    }
}
