using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Serialization;

namespace Movement.Components
{
    [RequireComponent(typeof(Rigidbody2D)), 
     RequireComponent(typeof(Animator)),
     RequireComponent(typeof(NetworkObject))]
    public sealed class FighterMovement : NetworkBehaviour, IMoveableReceiver, IJumperReceiver, IFighterReceiver
    {
        public float speed = 1.0f;
        public float jumpAmount = 1.0f;

        private Rigidbody2D _rigidbody2D;
        private Animator _animator;
        private NetworkAnimator _networkAnimator;
        private Transform _feet;
        private LayerMask _floor;

        private Vector3 _direction = Vector3.zero;
        private bool _grounded = true;
        
        private static readonly int AnimatorSpeed = Animator.StringToHash("speed");
        private static readonly int AnimatorVSpeed = Animator.StringToHash("vspeed");
        private static readonly int AnimatorGrounded = Animator.StringToHash("grounded");
        private static readonly int AnimatorAttack1 = Animator.StringToHash("attack1");
        private static readonly int AnimatorAttack2 = Animator.StringToHash("attack2");
        private static readonly int AnimatorHit = Animator.StringToHash("hit");
        private static readonly int AnimatorDie = Animator.StringToHash("die");

        void Start()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _networkAnimator = GetComponent<NetworkAnimator>();

            _feet = transform.Find("Feet");
            _floor = LayerMask.GetMask("Floor");
        }

        void Update()
        {
            if (!IsOwner) return;
            //
            //_grounded = Physics2D.OverlapCircle(_feet.position, 0.1f, _floor);
            //_animator.SetFloat(AnimatorSpeed, this._direction.magnitude);
            //_animator.SetFloat(AnimatorVSpeed, this._rigidbody2D.velocity.y);
            //_animator.SetBool(AnimatorGrounded, this._grounded);
            UpdateServerRpc();
        }

        [ServerRpc]
		void UpdateServerRpc()
		{
            _grounded = Physics2D.OverlapCircle(_feet.position, 0.1f, _floor);
            _animator.SetFloat(AnimatorSpeed, this._direction.magnitude);
            _animator.SetFloat(AnimatorVSpeed, this._rigidbody2D.velocity.y);
            _animator.SetBool(AnimatorGrounded, this._grounded);
        }

        void FixedUpdate()
        {
            //_rigidbody2D.velocity = new Vector2(_direction.x, _rigidbody2D.velocity.y);
            if (!IsOwner) return;
            FixedUpdateServerRpc();
        }

        [ServerRpc]
        void FixedUpdateServerRpc()
		{
            _rigidbody2D.velocity = new Vector2(_direction.x, _rigidbody2D.velocity.y);
        }

        public void Move(IMoveableReceiver.Direction direction)
        {
            ComputeMovementServerRpc(direction);
            //if (direction == IMoveableReceiver.Direction.None)
            //{
            //    this._direction = Vector3.zero;
            //    return;
            //}
            //
            //bool lookingRight = direction == IMoveableReceiver.Direction.Right;
            //_direction = (lookingRight ? 1f : -1f) * speed * Vector3.right;
            //transform.localScale = new Vector3(lookingRight ? 1 : -1, 1, 1);
        }

        [ServerRpc]
        void ComputeMovementServerRpc(IMoveableReceiver.Direction direction)
		{
            if (direction == IMoveableReceiver.Direction.None)
            {
                this._direction = Vector3.zero;
                return;
            }

            bool lookingRight = direction == IMoveableReceiver.Direction.Right;
            _direction = (lookingRight ? 1f : -1f) * speed * Vector3.right;
            transform.localScale = new Vector3(lookingRight ? 1 : -1, 1, 1);
        }

        public void Jump(IJumperReceiver.JumpStage stage)
        {
            ComputeJumpServerRpc(stage);
            //switch (stage)
            //{
            //    case IJumperReceiver.JumpStage.Jumping:
            //        if (_grounded)
            //        {
            //            float jumpForce = Mathf.Sqrt(jumpAmount * -2.0f * (Physics2D.gravity.y * _rigidbody2D.gravityScale));
            //            _rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            //        }
            //        break;
            //    case IJumperReceiver.JumpStage.Landing:
            //        break;
            //}
        }

        [ServerRpc]
        void ComputeJumpServerRpc(IJumperReceiver.JumpStage stage)
		{
            switch (stage)
            {
                case IJumperReceiver.JumpStage.Jumping:
                    if (_grounded)
                    {
                        float jumpForce = Mathf.Sqrt(jumpAmount * -2.0f * (Physics2D.gravity.y * _rigidbody2D.gravityScale));
                        _rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                    }
                    break;
                case IJumperReceiver.JumpStage.Landing:
                    break;
            }
        }

        public void Attack1()
        {
            //_networkAnimator.SetTrigger(AnimatorAttack1);
            ComputeAttack1ServerRpc();
        }

        [ServerRpc]
        private void ComputeAttack1ServerRpc()
		{
            _networkAnimator.SetTrigger(AnimatorAttack1);
        }

        public void Attack2()
        {
            //_networkAnimator.SetTrigger(AnimatorAttack2);
            ComputeAttack2ServerRpc();
        }

        [ServerRpc]
        private void ComputeAttack2ServerRpc()
		{
            _networkAnimator.SetTrigger(AnimatorAttack2);
        }

        public void TakeHit(int dmg) //SOLO SE LLAMA DESDE WEAPON, Y SOLO ACCEDE EL SERVIDOR
        {
            _networkAnimator.SetTrigger(AnimatorHit);
            //gameObject.GetComponent<HealthManager>().TakeDmg(dmg);
        }

        public void Die()
        {
            //_networkAnimator.SetTrigger(AnimatorDie); 
            ComputeDieServerRpc();
        }

        [ServerRpc]
        private void ComputeDieServerRpc()
		{
            _networkAnimator.SetTrigger(AnimatorDie); 
        }

        public void DesactivateCharacter()
		{
            gameObject.SetActive(false);
		}

        public void Revive()
		{
            //gameObject.SetActive(true);
            ComputeReviveServerRpc();
        }

        [ServerRpc]
        private void ComputeReviveServerRpc()
		{
            gameObject.SetActive(true);
        }
    }
}