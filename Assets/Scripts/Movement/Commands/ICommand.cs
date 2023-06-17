using Unity.Netcode;

namespace Movement.Commands
{
    public interface ICommand
    {
        public void Execute();

        public void Execute(ClientRpcParams clientRpcParams);
    }
}
