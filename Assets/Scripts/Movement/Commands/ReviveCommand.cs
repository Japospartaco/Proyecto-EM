using Movement.Components;
using Unity.Netcode;
using UnityEngine;

namespace Movement.Commands
{
    public class ReviveCommand : AFightCommand
    {
        public ReviveCommand(IFighterReceiver receiver) : base(receiver)
        {

        }

        public override void Execute()
        {
            throw new System.NotImplementedException();
        }

        public override void Execute(ClientRpcParams clientRpcParams)
        {
            Debug.Log("Intentando acceder al comando revivir.");
            Client.Revive(clientRpcParams);
        }

        public override void Execute(ulong clientId)
        {
            throw new System.NotImplementedException();
        }
    }
}
