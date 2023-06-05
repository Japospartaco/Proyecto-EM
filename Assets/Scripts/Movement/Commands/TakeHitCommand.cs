using Movement.Components;

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
    }
}