using Movement.Components;

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
    }
}
