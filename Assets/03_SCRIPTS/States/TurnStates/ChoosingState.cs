using MADP.Controllers;
using MADP.States.TurnStates.Interfaces;
using MADP.Utilities;
using MADP.Views;
using UnityEngine;

namespace MADP.States.TurnStates
{
    public class ChoosingState : BaseTurnState
    {
        private BoardController _boardController;
        
        public ChoosingState(TurnController controller) : base(controller)
        {
        }

        public override void EnterTurn()
        {
            base.EnterTurn();
        }

        public override void ExitTurn()
        {
            base.ExitTurn();
        }

        public override void ExecuteTurn()
        {
            base.ExecuteTurn();
            if (_turnController.IsPlayerTurn)
            {
                HandleInput();
            }
        }

        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                MouseDown();
            }
        }
        
        private void MouseDown()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            int cellLayer = LayerMask.GetMask(Constants.CellView);

            if (_turnController.CurrentDiceValue == 6)
            {
                int unitLayer = LayerMask.GetMask(Constants.UnitView);
                if (Physics.Raycast(ray, out RaycastHit unitHit, 500, unitLayer))
                {
                    var unitView =  unitHit.collider.GetComponent<UnitView>();
                    if (unitView != null)
                    {
                        _turnController.HandleUnitClicked(unitView.Model);
                        return;
                    }
                }
            }
            
            if (Physics.Raycast(ray, out RaycastHit cellHit, 500, cellLayer))
            {
                var cellView = cellHit.collider.GetComponent<CellView>();
                if (cellView != null)
                {
                    _turnController.HandleCellClicked(cellView.Model);
                    return;
                }
            }
            
            _turnController.DeselectCurrent();
        }
        
    }
}