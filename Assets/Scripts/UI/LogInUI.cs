using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LogInUI : MonoBehaviour
{
    [SerializeField] private GameObject LogInUIObject;
    [SerializeField] private GameObject LobbySelectorUIObject;
    [SerializeField] private GameObject FighterSelectorUIObject;
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private Button logInButton;
    

    private void Start()
    {
        logInButton.onClick.AddListener(OnLogInClicked);
        LogInUIObject.SetActive(false);
    }

    public void OnLogInClicked()
    {
        LogInUIObject.SetActive(false);
        LobbySelectorUIObject.SetActive(true);
        //FighterSelectorUIObject.SetActive(true);
        NetworkManager.Singleton.StartClient();
    }

    public string GetUsernameInput()
    {
        return usernameInput.text;
    }
}
