using Movement.Components;
using Unity.Netcode;

namespace Movement.Commands
{
    public abstract class AFightCommand : ICommand
    {
        protected readonly IFighterReceiver Client;

        protected AFightCommand(IFighterReceiver receiver)
        {
            Client = receiver;
        }

        public abstract void Execute();

        public abstract void Execute(ClientRpcParams clientRpcParams);        
    }
}