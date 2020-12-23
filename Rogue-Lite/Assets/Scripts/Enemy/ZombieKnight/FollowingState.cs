﻿using System.Dynamic;
using GoldPillowGames.Core;
using GoldPillowGames.Patterns;
using UnityEngine;

namespace GoldPillowGames.Enemy.ZombieKnight {
    public class FollowingState : EnemyState
    {
        #region Variables
        private readonly ZombieKnightController _enemyController;
        private readonly ITargetFollower _targetFollower;
        #endregion
        
        #region Methods
        public FollowingState(ZombieKnightController enemyController, FiniteStateMachine stateMachine, Animator anim) : base(stateMachine, anim)
        {
            _enemyController = enemyController;
            _targetFollower = new NavMeshTargetFollower(_enemyController.Agent);
        }

        public override void Enter()
        {
            base.Enter();

            _enemyController.Agent.isStopped = false;
            _targetFollower.SetTarget(_enemyController.Player);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            _targetFollower.Update(deltaTime);

            if (_enemyController.CanAttack && !_enemyController.IsThereAnObstacleInAttackRange())
            {
                stateMachine.SetState(new AttackState(_enemyController, stateMachine, anim));
            }
        }

        public override void Exit()
        {
            base.Exit();

            _enemyController.Agent.isStopped = true;
        }
        #endregion
    }
}