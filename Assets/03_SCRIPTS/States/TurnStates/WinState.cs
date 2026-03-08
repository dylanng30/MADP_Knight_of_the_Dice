using MADP.Controllers;
using UnityEngine;

namespace MADP.States.TurnStates
{
    public class WinState : BaseTurnState
    {
        public WinState(TurnController controller) : base(controller) {}
        public override void EnterTurn()
        {
            base.EnterTurn();
            
            Debug.Log($"Team {_turnController.CurrentTeam} đã chiến thắng");
            _turnController.TriggerGameEnd(_turnController.CurrentTeam);
        }
    }
}