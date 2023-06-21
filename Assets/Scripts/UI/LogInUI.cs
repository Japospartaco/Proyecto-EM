using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LogInUI : NetworkBehaviour
{
    [Header("UIs utilizadas")]
    [SerializeField] private GameObject LogInUIObject;
    [SerializeField] private GameObject LobbySelectorUIObject;
    [SerializeField] private GameObject FighterSelectorUIObject;

    [Header("Log in")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private Button logInButton;

    [Header("Objetos ocultos")]
    [SerializeField] private List<GameObject> gameObjectsToShow;

    

    private void Start()
    {
        logInButton.onClick.AddListener(OnLogInClicked);
        LogInUIObject.SetActive(false);
    }

    public void OnLogInClicked()
    {
        ShowAllHiddenObjects();

        NetworkManager.Singleton.StartClient();
        LogInUIObject.SetActive(false);
        LobbySelectorUIObject.SetActive(true);
    }

    void ShowAllHiddenObjects()
    {
        foreach (var gameObjectHidden in gameObjectsToShow)
        {
            gameObjectHidden.SetActive(true);
        }
    }

    public string GetUsernameInput()
    {
        return usernameInput.text;
    }
}
