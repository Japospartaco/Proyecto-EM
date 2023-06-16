using Movement.Components;
using UnityEngine;

namespace Movement.Commands
{
    public class ReviveCommand : AFightCommand
    {
        public ReviveCommand(IFighterReceiver receiver) : base(receiver)
        {

        }

        public override void Execute()
        {
            Debug.Log("Intentando acceder al comando revivir.");
            Client.Revive();
        }
    }
}
