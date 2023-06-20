using Movement.Components;
using Unity.Netcode;

namespace Movement.Commands
{
    public class Attack1Command : AFightCommand
    {
        public Attack1Command(IFighterReceiver receiver) : base(receiver)
        {
        }

        public override void Execute()
        {
            Client.Attack1();
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