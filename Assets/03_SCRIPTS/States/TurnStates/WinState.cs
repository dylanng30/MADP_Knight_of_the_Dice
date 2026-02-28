using MADP.Controllers;
using UnityEngine;

namespace MADP.States.TurnStates
{
    public class WinState : BaseTurnState
    {
        public WinState(TurnController controller) : base(controller)
        {
            
        }

        public override void EnterTurn()
        {
            base.EnterTurn();
            
            Debug.LogError($"Team {_turnController.CurrentTeam} đã chiến thắng");
        }

        public override void ExecuteTurn()
        {
            base.ExecuteTurn();
        }

        public override void ExitTurn()
        {
            base.ExitTurn();
        }

    }
}