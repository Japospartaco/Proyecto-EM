using Movement.Components;

namespace Movement.Commands
{
    public class ReviveCommand : AFightCommand
    {
        public ReviveCommand(IFighterReceiver receiver) : base(receiver)
        {

        }

        public override void Execute()
        {
            Client.Revive();
        }
    }
}
