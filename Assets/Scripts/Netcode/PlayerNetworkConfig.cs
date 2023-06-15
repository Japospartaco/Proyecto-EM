using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Netcode
{
    public class PlayerNetworkConfig : NetworkBehaviour
    {
        [SerializeField] private GameObject characterPrefab;
        private OnlinePlayers onlinePlayers;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;

            //#########BORRAR CUANDO SEA
            //InstantiateCharacterServerRpc(OwnerClientId);

            //OBTENEMOS EL "ID" DEL CLIENT Y EL "USERNAME" QUE HAYA ESCRITO POR PANTALLA
            string username = GameObject.FindGameObjectWithTag("UI Manager").GetComponent<LogInUI>().GetUsernameInput();
            ulong id = OwnerClientId;

            NewClientServerRpc(username, id);
        }

        [ServerRpc]
        private void NewClientServerRpc(string username, ulong id)
        {
            GameObject fighterObject = null;
            int currentLobbyId = -1;
            int idInLobby = -1;

            //INICIALIZACION DE LOS VALORES DE SU  COMPONENTE "PLAYER INFROMATION"
            gameObject.GetComponent<PlayerInformation>().InitializePlayer(id, username, fighterObject, currentLobbyId, idInLobby);
            GameObject.FindGameObjectWithTag("Game Manager").GetComponent<OnlinePlayers>().AddPlayer(id, gameObject);
        }
    

        //LLAMADA ANTIGUA  ############ BORRAR CUANDO NO SEA NECESARIA
        [ServerRpc]
        public void InstantiateCharacterServerRpc(ulong id)
        {
            GameObject characterGameObject = Instantiate(characterPrefab);
            characterGameObject.GetComponent<NetworkObject>().SpawnWithOwnership(id);
            characterGameObject.transform.SetParent(transform, false);
        }
    }
}
