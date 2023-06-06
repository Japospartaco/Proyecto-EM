using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ServerClientUI : MonoBehaviour
    {
        [SerializeField] private GameObject ServerClientUIObject;
        [SerializeField] private GameObject LogInUIObject;
        [SerializeField] private Button clientButton;
        [SerializeField] private Button serverButton;

        private void Start()
        {
            serverButton.onClick.AddListener(OnServerButtonClicked);
            clientButton.onClick.AddListener(OnClientButtonClicked);
            ServerClientUIObject.SetActive(true);
        }

        private void OnServerButtonClicked()
        {
            ServerClientUIObject.SetActive(false);
            NetworkManager.Singleton.StartServer();
        }

        private void OnClientButtonClicked()
        {
            ServerClientUIObject.SetActive(false);
            LogInUIObject.SetActive(true);
        }
    }
}