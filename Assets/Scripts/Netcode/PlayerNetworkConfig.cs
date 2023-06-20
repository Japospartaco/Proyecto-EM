using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Netcode
{
    public class PlayerNetworkConfig : NetworkBehaviour
    {
        [SerializeField] OnlinePlayers onlinePlayers;

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
        private void NewClientServerRpc(string username, ulong clientId)
        {
            GameObject fighterObject = null;
            int currentLobbyId = -1;
            int idInLobby = -1;

            //INICIALIZACION DE LOS VALORES DE SU  COMPONENTE "PLAYER INFROMATION"
            gameObject.GetComponent<PlayerInformation>().InitializePlayer(clientId, username, fighterObject, currentLobbyId, idInLobby);
            onlinePlayers = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<OnlinePlayers>();
            List<NetworkObject> list = onlinePlayers.ReturnNetworkObjectList();

            if (list.Count > 0)
            {
                NetworkObject.NetworkHide(list, clientId);
            }

            onlinePlayers.AddPlayer(clientId, gameObject);

        }
    }
}
