using Movement.Components;
using Unity.Netcode;

namespace Movement.Commands
{
    public class StopCommand : AMoveCommand
    {
        public StopCommand(IMoveableReceiver client) : base(client)
        {
        }

        public override void Execute()
        {
            ((IMoveableReceiver)Client).Move(IMoveableReceiver.Direction.None);
        }

        public override void Execute(ClientRpcParams client)
        {
            throw new System.NotImplementedException();
        }
    }
}
