using System;
using System.Collections;
using System.Threading;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Serialization;

namespace Movement.Components
{
    [RequireComponent(typeof(Rigidbody2D)),
     RequireComponent(typeof(Animator)),
     RequireComponent(typeof(NetworkObject))]
    public sealed class FighterMovement : NetworkBehaviour, IMoveableReceiver, IJumperReceiver, IFighterReceiver, IDashReceiver
    {
        // Variables públicas
        public float speed = 1.0f; // Velocidad del movimiento del luchador
        public float jumpAmount = 1.0f; // Cantidad de fuerza aplicada al saltar

        // Variables privadas
        private Rigidbody2D _rigidbody2D; // Referencia al componente Rigidbody2D
        private Animator _animator; // Referencia al componente Animator
        private NetworkAnimator _networkAnimator; // Referencia al componente NetworkAnimator
        private Transform _feet; // Referencia al transform del objeto "Feet"
        private LayerMask _floor; // Máscara de capas para detectar el suelo

        private Vector3 _direction = Vector3.zero; // Dirección del movimiento
        private bool _grounded = true; // Indica si el luchador está en el suelo
        private bool dashed = false; // Indica si el luchador ha realizado un dash

        // Hashes de los parámetros del Animator
        private static readonly int AnimatorSpeed = Animator.StringToHash("speed");
        private static readonly int AnimatorVSpeed = Animator.StringToHash("vspeed");
        private static readonly int AnimatorGrounded = Animator.StringToHash("grounded");
        private static readonly int AnimatorAttack1 = Animator.StringToHash("attack1");
        private static readonly int AnimatorAttack2 = Animator.StringToHash("attack2");
        private static readonly int AnimatorHit = Animator.StringToHash("hit");
        private static readonly int AnimatorDie = Animator.StringToHash("die");

        bool allowedMovement = true; // Indica si el movimiento está permitido

        public EventHandler<GameObject> DieEvent; // Evento que se dispara cuando el luchador muere

        public SemaphoreSlim EsperandoMorir = new SemaphoreSlim(0); // Semáforo para esperar la muerte del luchador


        public bool AllowedMovement
		{
            get { return allowedMovement; }
            set { allowedMovement = value; }
		}

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
            
            UpdateServerRpc();
        }

        [ServerRpc]
        public void UpdateServerRpc()
        {
            
            _grounded = Physics2D.OverlapCircle(_feet.position, 0.1f, _floor);
            if (_grounded)
            {
                dashed=false;
            }
            if (_rigidbody2D.velocity.magnitude > 90)
            {
                dashed = true;
            }

            _animator.SetFloat(AnimatorSpeed, this._direction.magnitude);
            _animator.SetFloat(AnimatorVSpeed, this._rigidbody2D.velocity.y);
            _animator.SetBool(AnimatorGrounded, this._grounded);
        }

        void FixedUpdate()
        {
            if (!IsOwner) return;
            FixedUpdateServerRpc();
        }

        [ServerRpc]
        public void FixedUpdateServerRpc()
        {
            _rigidbody2D.velocity = new Vector2(_direction.x, _rigidbody2D.velocity.y);
            
        }

        public void Move(IMoveableReceiver.Direction direction)
        {
            ComputeMoveServerRpc(direction);
        }

        [ServerRpc]
        public void ComputeMoveServerRpc(IMoveableReceiver.Direction direction)
        {
            if (direction == IMoveableReceiver.Direction.None || !allowedMovement)
            {
                this._direction = Vector3.zero;
                return;
            }

            bool lookingRight = direction == IMoveableReceiver.Direction.Right;
            _direction = (lookingRight ? 1f : -1f) * speed * Vector3.right;
            transform.localScale = new Vector3(lookingRight ? 1 : -1, 1, 1);
        }

        public void Dash(IDashReceiver.Stage stage)
        {
            ComputeDashServerRpc(stage);

        }

        [ServerRpc]
        public void ComputeDashServerRpc(IDashReceiver.Stage stage)
        {
            if (!allowedMovement) return;
            switch (stage)
            {
                case IDashReceiver.Stage.Dashed:
                    if (!_grounded && !dashed)
                    {
                        float dashForce = 100.0f;
                        Vector2 forceDirection = new Vector2(transform.localScale.x, 0f).normalized;
                        Vector2 dashForceVector = forceDirection * dashForce;
                        _rigidbody2D.AddForce(dashForceVector, ForceMode2D.Impulse);
                    }
                    break;
                case IDashReceiver.Stage.Posible:
                    break;
            }
        }

        public void Jump(IJumperReceiver.JumpStage stage)
        {
            ComputeJumpServerRpc(stage);
        }

        [ServerRpc]
        public void ComputeJumpServerRpc(IJumperReceiver.JumpStage stage)
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
            ComputeAttack1ServerRpc();
        }

        [ServerRpc]
        private void ComputeAttack1ServerRpc()
        {
            _networkAnimator.SetTrigger(AnimatorAttack1);
        }

        public void Attack2()
        {
            ComputeAttack2ServerRpc();
        }

        [ServerRpc]
        private void ComputeAttack2ServerRpc()
        {
            _networkAnimator.SetTrigger(AnimatorAttack2);
        }

        public void TakeHit(int dmg) //SOLO SE LLAMA DESDE WEAPON, Y SOLO ACCEDE EL SERVIDOR
        {
            if (!IsServer) return;

            _networkAnimator.SetTrigger(AnimatorHit);
            gameObject.GetComponent<HealthManager>().TakeDmg(dmg);
        }

        public void Die()
        {
            //_networkAnimator.SetTrigger(AnimatorDie); 
            if (!IsServer) return;

            _networkAnimator.SetTrigger(AnimatorDie);
        }

        public void DesactivateCharacter()
		{
            if (!IsServer) return;

            DesactivateCharacterClientRpc();

            Debug.Log("Llamando al evento de morir");
            gameObject.SetActive(false);
            DieEvent?.Invoke(this, gameObject);
        }

        [ClientRpc]
        public void DesactivateCharacterClientRpc(ClientRpcParams clientRpcParams = default)
        {

            gameObject.SetActive(false);
            EsperandoMorir.Release();

        }

        public void Revive(ClientRpcParams clientRpcParams)
        {
            ReviveClientRpc(clientRpcParams);

            gameObject.GetComponent<HealthManager>().ResetHealth();
            gameObject.SetActive(true);
        }

        [ClientRpc]
        void ReviveClientRpc(ClientRpcParams clientRpcParams = default)
        {
            EsperandoMorir.Wait();
            gameObject.SetActive(true);
        }

        public void ActivateCharacter(ulong clientId)
        {
            gameObject.GetComponent<NetworkObject>().NetworkShow(clientId);
        }
    }
}