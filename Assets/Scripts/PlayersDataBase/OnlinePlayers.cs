using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Movement.Commands;
using UnityEngine.TextCore.Text;
using Movement.Components;

// clase encargada de gestionar los clientes que se vayan uniendo
public class OnlinePlayers : NetworkBehaviour
{
    [Header("Diccionario <idClient, player>")]
    [SerializeField] Dictionary<ulong, GameObject> onlinePlayers = new(); //diccionario que clasifica los clientes por su id

    public Dictionary<ulong, GameObject> OnlinePlayersDictionary
    {
        get { return onlinePlayers; }
    }

    private void Start()
    {
        NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
    }

    //metodo que devuelve una lista con todos los network objects de los clientes en el diccionario
    public List<NetworkObject> ReturnNetworkObjectList()
    {
        List<NetworkObject> networkObjectList = new List<NetworkObject>();

        foreach (var id in onlinePlayers.Keys)
        {
            GameObject fighter = onlinePlayers[id].GetComponent<PlayerInformation>().FighterObject;
            if (fighter != null)
            {
                NetworkObject networkObject = fighter.GetComponent<NetworkObject>();
                networkObjectList.Add(networkObject);
            }
        }

        return networkObjectList;
    }

    //metodo para añadir clientes al diciconario
    public void AddPlayer(ulong id, GameObject player)
    {
        if (!IsServer) return;

        onlinePlayers[id] = player;

    }

    //METODO PARA OBTENER UNA LISTA DE GAMEOBJECTS "PLAYER" A PARTIR DE UNA LISTA DE IDS
    public List<GameObject> GetPlayerListFromIds(List<ulong> id_players)
    {
        if (!IsServer) return null;
        List<GameObject> players = new List<GameObject>();

        foreach (var id in id_players)
        {
            players.Add(onlinePlayers[id]);
        }

        return players;
    }

    //METODO PARA OBTENER UNA LISTA DE GAMEOBJECTS "FIGHTER" A PARTIR DE UNA LISTA DE IDS
    public List<GameObject> GetFighterListFromIds(List<ulong> id_players)
    {
        if (!IsServer) return null;
        List<GameObject> fighters = new List<GameObject>();

        foreach (var id in id_players)
        {
            fighters.Add(onlinePlayers[id].GetComponent<PlayerInformation>().FighterObject);
        }

        return fighters;
    }

    //METODO PARA DEVOLVER UN GAMEOBJECT "PLAYER" ESPECIFICANDO SU ID
    public GameObject ReturnPlayerGameObject(ulong id)
    {
        if (!IsServer) return null;
        return onlinePlayers[id];
    }

    //METODO PARA DEVOLVER UN COMPONENTE "PLAYER INFORMATION" ESPECIFICANDO SU ID
    public PlayerInformation ReturnPlayerInformation(ulong id)
    {
        if (!IsServer) return null;
        return onlinePlayers[id].GetComponent<PlayerInformation>();
    }

    //metodo encargado de la gestion de las desconexiones
    public void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        //primero se comprueba si el cliente estaba dentro de una sala, de no ser asi, solo se elimina del diccionario
        int currentLobbyId = onlinePlayers[clientId].GetComponent<PlayerInformation>().CurrentLobbyId;
        if (currentLobbyId == -1)
        {
            onlinePlayers.Remove(clientId);
            return;
        }

        //obtenemos la sala donde se encuentre
        Lobby lobby = GetComponent<LobbyManager>().GetLobbyFromId(currentLobbyId);

        GameObject fighter = null;

        //comprobamos si la partida en la lobby esta empezada
        if (lobby.IsStarted)
        {
            //obtenemos el luchador del cliente
            fighter = onlinePlayers[clientId].GetComponent<PlayerInformation>().FighterObject;
            if (fighter)
            {
                //se mata al luchador
                new TakeHitCommand(fighter.GetComponent<FighterMovement>(), 999).Execute();

                //seteamos la variable isDisconnected a true 
                fighter.GetComponent<FighterInformation>().IsDisconnected = true;
            }
        }
        else //de no haber empezado la partida, se elimina de la lobby y se elimina del diccionario
        {
            lobby.RemovePlayerFromLobby(clientId);
            onlinePlayers.Remove(clientId);
        }
    }


}