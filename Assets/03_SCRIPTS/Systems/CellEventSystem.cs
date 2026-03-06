using System.Collections;
using MADP.Models;
using MADP.Models.UnitActions;
using MADP.Views;
using MADP.Views.Unit;
using Unity.VisualScripting;
using UnityEngine;

namespace MADP.Systems
{
    public class CellEventSystem : MonoBehaviour
    {
        private void OnEnable()
        {
            ActionSystem.AttachPerformer<CellEventUA>(ExecuteCellEvent);
        }

        private void OnDisable()
        {
            ActionSystem.DetachPerformer<CellEventUA>();
        }

        private IEnumerator ExecuteCellEvent(CellEventUA cellEventUA)
        {
            UnitModel unitModel = cellEventUA.UnitModel;
            UnitView unitView = cellEventUA.UnitView;
            CellModel cellModel = cellEventUA.CellModel;
            CellView cellView = cellEventUA.CellView;
            cellEventUA.CellEvent.Execute(unitModel, unitView, cellModel, cellView);
            yield return new WaitForEndOfFrame();
        }
    }
}