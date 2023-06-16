using Unity.Netcode;

namespace Movement.Components
{
    public interface IFighterReceiver : IRecevier
    {
        public void Attack1();
        public void Attack2();
        public void TakeHit(int dmg);
        public void Die();
        public void Revive(ClientRpcParams clientRpcParams);
    }
}