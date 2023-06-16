using Movement.Components;
using Unity.Netcode;

namespace Movement.Commands
{
    public class WalkLeftCommand : AMoveCommand
    {
        public WalkLeftCommand(IMoveableReceiver client) : base(client)
        {
        }

        public override void Execute()
        {
            ((IMoveableReceiver)Client).Move(IMoveableReceiver.Direction.Left);
        }

        public override void Execute(ClientRpcParams client)
        {
            throw new System.NotImplementedException();
        }
    }
}
