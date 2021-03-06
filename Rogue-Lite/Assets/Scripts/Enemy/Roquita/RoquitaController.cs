﻿using System;
using System.Linq;
using GoldPillowGames.Core;
using GoldPillowGames.Patterns;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using Photon.Pun;

namespace GoldPillowGames.Enemy.Roquita
{
    public class RoquitaController : EnemyController
    {
        #region Variables
        [SerializeField] private float velocity = 5;
        [SerializeField] private float distanceToAttack = 5;
        [SerializeField] private float detectAngle;
        [SerializeField] private float pushAttackForce = 15;
        [SerializeField] private float minTimeToJump = 3;
        [SerializeField] private float maxTimeToJump = 8;
        [SerializeField] private float jumpAttackRadius = 5;
        [SerializeField] private int quickHandDamage = 15;
        [SerializeField] private int strongHandDamage = 20;
        [SerializeField] private int jumpDamage = 25;
        [SerializeField] private GameObject body;
        [SerializeField] private RoquitaParticlesController earthquakeParticles;
        [SerializeField] private GameObject healthBar;
        [SerializeField] private RoquitaHandController quickHand;
        [SerializeField] private RoquitaHandController strongHand;
        [SerializeField] private ParticleSystem landingParticles;
        private Animator _anim;
        private AgentPropeller _propeller;
        private Collider _collider;
        private PhotonView photonView;
        private PlayerController[] _players;
        #endregion

        #region Properties
        public Transform Player { get; private set; }
        public NavMeshAgent Agent { get; private set; }
        public float Velocity => velocity;
        public float TimeToJump => Random.Range(minTimeToJump, maxTimeToJump);

        private float DistanceFromPlayer => Vector3.Distance(transform.position,
            Player.transform.position);

        private Vector3 DirectionToPlayer =>
            (Player.position - transform.position).normalized;

        public bool PlayerIsInRange => DistanceFromPlayer <= distanceToAttack;
        public bool CanAttack => (PlayerIsInRange && CanSeePlayer() && !IsThereAnObstacleInAttackRange());
        #endregion

        #region Methods
        protected override void Awake()
        {
            base.Awake();

            photonView = GetComponent<PhotonView>();
            _players = FindObjectsOfType<PlayerController>();
            Agent = GetComponent<NavMeshAgent>();
            Player = FindObjectOfType<PlayerController>().transform;
            _anim = GetComponentInChildren<Animator>();
            _propeller = new AgentPropeller(Agent);
            _collider = GetComponent<Collider>();

            if (!photonView.IsMine && Config.data.isOnline)
            {
                Agent.enabled = false;
            }
        }

        protected override void Start()
        {
            quickHand.SetDamage(quickHandDamage);
            strongHand.SetDamage(strongHandDamage);

            
            base.Start();

            if (!photonView.IsMine && Config.data.isOnline)
                return;

            if (Config.data.isOnline)
            {
                CheckClosestPlayer();
            }

            stateMachine.SetInitialState(new FollowingState(this, stateMachine, _anim));
            transform.forward = DirectionToPlayer;
            
            
        }

        [PunRPC]
        protected override void GiveGold()
        {
            base.GiveGold();
        }

        protected override void CheckClosestPlayer()
        {
            if (_players.Length < 2)
                _players = FindObjectsOfType<PlayerController>();

            float distance = Mathf.Infinity;
            PlayerController closestPlayer = null;

            foreach (PlayerController p in _players)
            {
                if (p != null)
                {
                    if ((p.transform.position - transform.position).magnitude < distance)
                    {
                        distance = (p.transform.position - transform.position).magnitude;
                        closestPlayer = p;
                    }
                }

            }

            if (closestPlayer != null)
            {
                if (Player != closestPlayer)
                {
                    Player = closestPlayer.transform;
                }
            }
        }

        protected override void Update()
        {
            if (!photonView.IsMine && Config.data.isOnline)
                return;

            base.Update();

            
        }

        protected override void FixedUpdate()
        {
            if (!photonView.IsMine && Config.data.isOnline)
                return;
            base.FixedUpdate();

            if (Config.data.isOnline)
            {
                CheckClosestPlayer();
            }

            _propeller.Update(Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, distanceToAttack);
            
            Gizmos.color = Color.blue;
            var position = transform.position;
            var forward = transform.forward;
            Gizmos.DrawLine(position, position + Quaternion.Euler(0, detectAngle / 2, 0) * forward * distanceToAttack);
            Gizmos.DrawLine(position, position + Quaternion.Euler(0, -detectAngle / 2, 0) * forward * distanceToAttack);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, jumpAttackRadius);
        }

        private bool IsThereAnObstacleInAttackRange()
        {
            if (!Physics.Raycast(transform.position, DirectionToPlayer, out var hitInfo,
                distanceToAttack, LayerMask.GetMask("Ground", "Player")))
            {
                return false;
            }
            return !hitInfo.collider.CompareTag("Player");
        }

        private bool CanSeePlayer()
        {
            Vector2 forward = new Vector2(transform.forward.x, transform.forward.z);
            var playerPosition = Player.position;
            var selfPosition = transform.position;
            
            Vector2 toPlayer = new Vector2(playerPosition.x - selfPosition.x,
                playerPosition.z - selfPosition.z);
            
            return (Vector2.Angle(forward, toPlayer) < (detectAngle / 2));
        }

        public void HideRoquita()
        {
            body.SetActive(false);
        }

        public void ShowRoquita()
        {
            body.SetActive(true);
        }
        
        public void JumpAreaLandingAttack()
        {
            var playerCollidersHit = Physics.OverlapSphere(transform.position, jumpAttackRadius,
                LayerMask.GetMask("Player")).Where(col => col.CompareTag("Player"));
            foreach (var playerCollider in playerCollidersHit)
            {
                playerCollider.GetComponent<PlayerController>().TakeDamage(jumpDamage,
                    (playerCollider.transform.position - transform.position).normalized);
            }
            
            earthquakeParticles.Stop();
            landingParticles.Play();
            
            CameraShaker.Shake(0.4f, 3, 4);
        }

        //[PunRPC]
        //public void SyncTakeDamage()
        //{

        //}

        public void DisableCollider()
        {
            _collider.enabled = false;
            healthBar.SetActive(false);
        }

        public void EnableCollider()
        {
            _collider.enabled = true;
            healthBar.SetActive(true);
        }

        public void InitAttackStrongHand()
        {
            strongHand.InitAttack();
        }
        
        public void FinishAttackStrongHand()
        {
            strongHand.FinishAttack();
        }
        
        public void InitAttackQuickHand()
        {
            quickHand.InitAttack();
        }
        
        public void FinishAttackQuickHand()
        {
            quickHand.FinishAttack();
        }
        
        public void PrepareToLand()
        {
            var currentPosition = transform.position;
            Vector3 playerPosition;
            if (!Config.data.isOnline)
                playerPosition = Player.position;
            else
            {
                CheckClosestPlayer();
                playerPosition = Player.position;
            }
                
            
            Agent.Warp(new Vector3(playerPosition.x, currentPosition.y, playerPosition.z));
            
            earthquakeParticles.Play(transform.position);
        }
        
        public void AttackPush(float time)
        {
            _propeller.StartPush(time, pushAttackForce * transform.forward);
        }

        [PunRPC]
        protected override void Die()
        {
            base.Die();
            
            _collider.enabled = false;
            Agent.enabled = false;
            _anim.enabled = false;
            enabled = false;
        }

        public void DiePublic()
        {
            Die();
        }

        [PunRPC]
        public override void ReceiveDamage(float damage)
        {
            if (!photonView.IsMine && Config.data.isOnline)
                return;

            base.ReceiveDamage(damage);
            
            if (health <= 0)
            {
                stateMachine.SetState(new DeathState(this, stateMachine, _anim));
            }
        }
        
        public void ResetState(AnimatedState state)
        { 
            var currentState = _anim.GetCurrentAnimatorStateInfo(0);
            var stateName = currentState.fullPathHash;
            
            _anim.Play(stateName,0, 0.0f);
            
            stateMachine.SetState(state);
        }
        #endregion
    }
}