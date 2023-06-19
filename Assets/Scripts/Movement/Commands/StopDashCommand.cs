using Movement.Components;
using Unity.Netcode;

namespace Movement.Commands
{
    public class StopDashCommand : AMoveCommand
    {
        public StopDashCommand(IDashReceiver client) : base(client)
        {
        }

        public override void Execute()
        {
            ((IDashReceiver)Client).Dash(IDashReceiver.Stage.Posible);
        }

        public override void Execute(ClientRpcParams client)
        {
            throw new System.NotImplementedException();
        }
    }
}
