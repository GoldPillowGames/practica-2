﻿using GoldPillowGames.Core;
using GoldPillowGames.Patterns;
using UnityEngine;

namespace GoldPillowGames.Enemy.SinCabeza
{
    public class HurtState : EnemyState
    {
        #region Variables
        private readonly SinCabezaController _enemyController;
        private readonly MethodDelayer _nextStateDelayer;
        #endregion
        
        #region Methods
        public HurtState(SinCabezaController enemyController, FiniteStateMachine stateMachine, Animator anim) : base(stateMachine, anim)
        {
            animationBoolParameterSelector.Add("IsReceivingDamage");
            _enemyController = enemyController;
            _nextStateDelayer = new MethodDelayer(GoToNextState);
        }

        public override void Enter()
        {
            base.Enter();
            
            _nextStateDelayer.SetNewDelay(_enemyController.TimeDefenseless);
        }

        public override void FixedUpdate(float deltaTime)
        {
            base.FixedUpdate(deltaTime);
            
            _nextStateDelayer.Update(deltaTime);
        }

        private void GoToNextState()
        {
            if (_enemyController.CanAttack)
            {
                stateMachine.SetState(new AttackState(_enemyController, stateMachine, anim));
            }
            else
            {
                stateMachine.SetState(new FollowingState(_enemyController, stateMachine, anim));
            }
        }
        #endregion
    }
}