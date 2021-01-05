﻿using GoldPillowGames.Core;
using GoldPillowGames.Patterns;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace GoldPillowGames.Enemy.SinCabeza
{
    public class SinCabezaController : EnemyController
    {
        #region Variables
        [SerializeField] private float velocity = 5;
        [SerializeField] private float distanceToAttack = 5;
        [SerializeField] private float distanceToStopAttacking = 8;
        [SerializeField] private float rotationSpeed = 10;
        [SerializeField] private float timeDefenseless = 1;
        [SerializeField] private float timeBetweenOrb = 1;
        [SerializeField] private GameObject orbPrefab;
        [SerializeField] private Transform orbSpawner;
        private Animator _anim;
        private AgentPropeller _propeller;
        private BoxCollider _collider;
        #endregion

        #region Properties
        public Transform Player { get; private set; }
        
        public NavMeshAgent Agent { get; private set; }
        public Transform Transform => transform;
        public float Velocity => velocity;
        public float RotationSpeed => rotationSpeed;
        public float TimeDefenseless => timeDefenseless;
        public float TimeForNextOrb => timeBetweenOrb;
        private float DistanceFromPlayer => Vector3.Distance(Transform.position,
            Player.transform.position);
        private Vector3 DirectionToPlayer =>
            (Player.position - Transform.position).normalized;
        private bool PlayerIsInRange => DistanceFromPlayer <= distanceToAttack;
        public bool CanAttack => (PlayerIsInRange && !IsThereAnObstacleInAttackRange());
        #endregion

        #region Methods
        protected override void Awake()
        {
            base.Awake();

            Agent = GetComponent<NavMeshAgent>();
            Player = FindObjectOfType<PlayerController>().transform;
            _anim = GetComponent<Animator>();
            _propeller = new AgentPropeller(Agent);
            _collider = GetComponent<BoxCollider>();
        }

        protected override void Start()
        {
            base.Start();
            
            stateMachine.SetInitialState(new FollowingState(this, stateMachine, _anim));
            transform.forward = -DirectionToPlayer;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            
            _propeller.Update(Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, distanceToAttack);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, distanceToStopAttacking);
        }

        private bool IsThereAnObstacleInAttackRange()
        {
            if (!Physics.Raycast(Transform.position, DirectionToPlayer, out var hitInfo,
                distanceToAttack, LayerMask.GetMask("Ground", "Player")))
            {
                return false;
            }
            return !hitInfo.collider.CompareTag("Player");
        }
        
        public void ThrowOrb()
        {
            var orb = ObjectPool.Instance.GetObject(orbPrefab);
            orb.GetComponent<OrbController>().Init(orbSpawner.position, transform.forward);
        }

        public override void Push(float time, float force, Vector3 direction)
        {
            _propeller.StartPush(time, force * direction);
        }
        
        protected override void Die()
        {
            base.Die();
            
            gameObject.layer = LayerMask.NameToLayer("DeathEnemy");
            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                child.gameObject.layer = LayerMask.NameToLayer("DeathEnemy");
            }
            _collider.enabled = false;
            Agent.enabled = false;
            _anim.enabled = false;
            enabled = false;
        }
        
        public override void ReceiveDamage(float damage)
        {
            base.ReceiveDamage(damage);

            if (health > 0)
            {
                stateMachine.SetState(new HurtState(this, stateMachine, _anim));
            }
            else
            {
                stateMachine.SetState(new DeathState(this, stateMachine, _anim));
            }
        }

        #endregion
    }
}