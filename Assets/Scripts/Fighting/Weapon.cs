using Movement.Components;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Serialization;
using Movement.Commands;

namespace Fighting
{
    public class Weapon : NetworkBehaviour
    {
        public Animator effectsPrefab;
        private static readonly int Hit03 = Animator.StringToHash("hit03");
        [SerializeField] private int dmg;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsServer) return;

            GameObject otherObject = collision.gameObject;
            // Debug.Log($"Sword collision with {otherObject.name}");

            Animator effect = Instantiate(effectsPrefab);
            effect.transform.position = collision.GetContact(0).point;
            effect.SetTrigger(Hit03);

            // TODO: Review if this is the best way to do this
            IFighterReceiver enemy = otherObject.GetComponent<IFighterReceiver>();
            HealthManager healthManager = otherObject.GetComponent<HealthManager>();

            if(enemy != null)
			{
                new TakeHitCommand(enemy, dmg);
            }
        }
    }
}
