using Movement.Commands;
using Movement.Components;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ActivateCharacterCommand : AFightCommand
{
    public ActivateCharacterCommand(IFighterReceiver receiver) : base(receiver)
    {

    }

    public override void Execute()
    {
        throw new System.NotImplementedException();
    }

    public override void Execute(ulong clientId)
    {
        Debug.Log("Intentando acceder al comando activar character.");
        Client.ActivateCharacter(clientId);
    }

    public override void Execute(ClientRpcParams clientRpcParams)
    {
        throw new System.NotImplementedException();
    }

}
