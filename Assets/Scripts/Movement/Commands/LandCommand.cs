using Movement.Components;
using Unity.Netcode;

namespace Movement.Commands
{
    class LandCommand : AMoveCommand
    {
        public LandCommand(IJumperReceiver client) : base(client)
        {
        }

        public override void Execute()
        {
            ((IJumperReceiver)Client).Jump(IJumperReceiver.JumpStage.Landing);
        }

        public override void Execute(ClientRpcParams client)
        {
            throw new System.NotImplementedException();
        }
    }
}
