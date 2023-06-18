using Movement.Components;

namespace Movement.Commands
{
    public class DashLeftCommand : AMoveCommand
    {
        public DashLeftCommand(IDashReceiver client) : base(client)
        {
        }

        public override void Execute()
        {
            ((IDashReceiver)Client).Dash(IDashReceiver.Stage.Dashed);
        }
    }
}
