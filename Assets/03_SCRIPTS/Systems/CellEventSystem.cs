using System.Collections;
using MADP.Models.UnitActions;
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
            
            cellEventUA.CellEvent.Execute(cellEventUA.UnitModel, cellEventUA.CellModel);
            yield return new WaitForEndOfFrame();
        }
    }
}