using Movement.Components;
using Unity.Netcode;

namespace Movement.Commands
{
    public class DieCommand : AFightCommand
    {
        public DieCommand(IFighterReceiver receiver) : base(receiver)
        {
        }

        public override void Execute()
        {
            Client.Die();
        }

        public override void Execute(ClientRpcParams clientRpcParams)
        {
            throw new System.NotImplementedException();
        }

        public override void Execute(ulong clientId)
        {
            throw new System.NotImplementedException();
        }
    }
}