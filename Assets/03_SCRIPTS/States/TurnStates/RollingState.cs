using System.Collections;
using MADP.Controllers;
using UnityEngine;

namespace MADP.States.TurnStates
{
    public class RollingState  : BaseTurnState
    {
        private float timer;
        private bool roled;
        public RollingState(TurnController controller) : base(controller)
        {
        }

        public override void EnterTurn()
        {
            base.EnterTurn();
            timer = 2f;
            roled =  false;
        }

        public override void ExecuteTurn()
        {
            if (_turnController.IsPlayerTurn && Input.GetKeyDown(KeyCode.Space))
            {
                _turnController.RollDice();
            }
            else if (!_turnController.IsPlayerTurn)
            {
                if (CanBotRole())
                {
                    _turnController.RollDice();
                    roled = true;
                }
            }
            
            //_turnController.RollDice();
        }

        private bool CanBotRole()
        {
            timer -= Time.deltaTime;
            return timer <= 0f && !roled;
        }
    }
}

