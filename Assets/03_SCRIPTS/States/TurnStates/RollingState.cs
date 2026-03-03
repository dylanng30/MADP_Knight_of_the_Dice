using System.Collections;
using MADP.Controllers;
using UnityEngine;

namespace MADP.States.TurnStates
{
    public class RollingState  : BaseTurnState
    {
        private float _timer;
        private bool _roled;
        public RollingState(TurnController controller) : base(controller)
        {
        }

        public override void EnterTurn()
        {
            base.EnterTurn();
            _timer = 1f;
            _roled =  false;
        }

        public override void ExecuteTurn()
        {
            if (_turnController.IsPlayerTurn)
            {
                if(Input.GetKeyDown(KeyCode.Space))
                    _turnController.RollDice();
            }
            else
            {
                if (CanBotRole())
                {
                    _turnController.RollDice();
                    _roled = true;
                }
            }
        }
        
        public override void OnInteract()
        {
            if (_roled) return;

            _roled = true;
            _turnController.RollDice();
            _turnController.SetRollButtonVisibility(false);
        }

        private bool CanBotRole()
        {
            _timer -= Time.deltaTime;
            return _timer <= 0f && !_roled;
        }
    }
}

