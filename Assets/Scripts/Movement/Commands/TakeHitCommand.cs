using Movement.Components;
using Unity.Netcode;

namespace Movement.Commands
{
    public class TakeHitCommand : AFightCommand
    {

        int dmg; 

        public TakeHitCommand(IFighterReceiver receiver, int dmg) : base(receiver)
        {
            this.dmg = dmg;
        }

        public override void Execute()
        {
            Client.TakeHit(dmg);
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