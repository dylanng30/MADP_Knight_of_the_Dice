using MADP.Controllers;
using MADP.States.TurnStates.Interfaces;
using MADP.Utilities;
using MADP.Views;
using MADP.Views.Unit;
using UnityEngine;

namespace MADP.States.TurnStates
{
    public class ChoosingState : BaseTurnState
    {
        public ChoosingState(TurnController controller) : base(controller)
        {
        }

        public override void EnterTurn()
        {
            base.EnterTurn();
            if (!_turnController.IsPlayerTurn)
            {
                _turnController.HandleBotTurn();
            }
        }

        public override void ExecuteTurn()
        {
            base.ExecuteTurn();
            if (_turnController.IsPlayerTurn)
            {
                HandleInput();
            }
        }

        public override void OnInteract()
        {
            _turnController.EndTurn();
            _turnController.SetEndTurnButtonVisibility(false);
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

            /*if (_turnController.CurrentDiceValue == 6)
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
            }*/
            
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