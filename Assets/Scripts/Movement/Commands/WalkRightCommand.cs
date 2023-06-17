using Movement.Components;
using Unity.Netcode;

namespace Movement.Commands
{
    public class WalkRightCommand : AMoveCommand
    {
        public WalkRightCommand(IMoveableReceiver client) : base(client)
        {
        }

        public override void Execute()
        {
            ((IMoveableReceiver)Client).Move(IMoveableReceiver.Direction.Right);
        }

        public override void Execute(ClientRpcParams client)
        {
            throw new System.NotImplementedException();
        }
    }
}
