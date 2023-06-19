using Movement.Components;
using Unity.Netcode;

namespace Movement.Commands
{
    public class DashCommand : AMoveCommand
    {
        public DashCommand(IDashReceiver client) : base(client)
        {
        }

        public override void Execute()
        {
            ((IDashReceiver)Client).Dash(IDashReceiver.Stage.Dashed);
        }

        public override void Execute(ClientRpcParams client)
        {
            throw new System.NotImplementedException();
        }
    }
}
