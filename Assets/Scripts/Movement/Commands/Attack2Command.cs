using Movement.Components;
using Unity.Netcode;

namespace Movement.Commands
{
    public class Attack2Command : AFightCommand
    {
        public Attack2Command(IFighterReceiver receiver) : base(receiver)
        {
        }

        public override void Execute()
        {
            Client.Attack2();
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