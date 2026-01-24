using MADP.Controllers;
using MADP.States.TurnStates.Interfaces;
using UnityEngine;

namespace MADP.States.TurnStates
{
    public abstract class BaseTurnState : ITurnState
    {
        protected TurnController _turnController;
        public BaseTurnState(TurnController turnController)
        {
            _turnController = turnController;
        }

        public virtual void EnterTurn()
        {
            //Debug.Log($"Entering {GetType().Name} turn's {_turnController.CurrentTeam}");
        }

        public virtual void ExecuteTurn()
        {
            
        }

        public virtual void ExitTurn()
        {
            
        }

    }
}