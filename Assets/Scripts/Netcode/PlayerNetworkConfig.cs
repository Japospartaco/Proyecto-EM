using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Netcode
{
    public class PlayerNetworkConfig : NetworkBehaviour
    {
        [SerializeField] private GameObject characterPrefab;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            //InstantiateCharacterServerRpc(OwnerClientId);

            string username = GameObject.FindGameObjectWithTag("UI Manager").GetComponent<LogInUI>().GetUsernameInput();
            ulong id = OwnerClientId;

            NewClientServerRpc(username, id);
        }

        [ServerRpc]
        private void NewClientServerRpc(string username, ulong id)
        {
            GameObject fighterObject = null;
            int currentLobbyId = -1;

            gameObject.GetComponent<PlayerInformation>().InitializePlayer(id, username, fighterObject, currentLobbyId);
        }
    


        [ServerRpc]
        public void InstantiateCharacterServerRpc(ulong id)
        {
            GameObject characterGameObject = Instantiate(characterPrefab);
            characterGameObject.GetComponent<NetworkObject>().SpawnWithOwnership(id);
            characterGameObject.transform.SetParent(transform, false);
        }
    }
}
