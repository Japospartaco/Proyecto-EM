using Movement.Components;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Serialization;
using Movement.Commands;
using Unity.Netcode.Components;

namespace Fighting
{
    public class Weapon : NetworkBehaviour
    {
        public Animator effectsPrefab;
        private static readonly int Hit03 = Animator.StringToHash("hit03");
        [SerializeField] private int dmg;
        public int Dmg
		{
            get { return dmg; }
            set { dmg = value; }
		}

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsServer) return;

            GameObject otherObject = collision.gameObject;
            // Debug.Log($"Sword collision with {otherObject.name}");

            Animator effect = Instantiate(effectsPrefab);
            effect.transform.position = collision.GetContact(0).point;
         
            effect.GetComponent<NetworkObject>().Spawn();
            effect.GetComponent<NetworkAnimator>().SetTrigger(Hit03);

            // TODO: Review if this is the best way to do this
            IFighterReceiver enemy = otherObject.GetComponent<IFighterReceiver>();
            if(enemy != null)
			{
                new TakeHitCommand(enemy, dmg).Execute();
            }
        }
    }
}
