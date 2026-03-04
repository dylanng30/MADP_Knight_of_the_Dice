using System;
using MADP.Controllers;
using MADP.States.TurnStates.Interfaces;
using MADP.Systems;
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
            else if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                    MouseDown();
            }
        }
        
        private void MouseDown()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            int cellLayer = LayerMask.GetMask(Constants.CellView);
            
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