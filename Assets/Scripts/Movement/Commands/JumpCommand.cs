using Movement.Components;
using Unity.Netcode;

namespace Movement.Commands
{
    class JumpCommand : AMoveCommand
    {
        public JumpCommand(IJumperReceiver client) : base(client)
        {
        }

        public override void Execute()
        {
            ((IJumperReceiver)Client).Jump(IJumperReceiver.JumpStage.Jumping);
        }

        public override void Execute(ClientRpcParams client)
        {
            throw new System.NotImplementedException();
        }
    }
}
