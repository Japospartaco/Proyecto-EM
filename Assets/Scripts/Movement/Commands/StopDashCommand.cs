using Movement.Components;

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
    }
}
