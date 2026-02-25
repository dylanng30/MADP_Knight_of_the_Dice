using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MADP.Utilities;
using MADP.Views;
using MADP.Views.UnitInfo;
using UnityEngine;

namespace MADP.Controllers
{
    public class UnitInfoController : MonoBehaviour
    {
        [SerializeField] private UnitInfoView unitInfoView;

        private void Awake()
        {
            unitInfoView.Clear();
        }
        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                OnMouseDown();
            }
        }

        private void OnMouseDown()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            int cellLayer = LayerMask.GetMask(Constants.CellView);

            if (Physics.Raycast(ray, out RaycastHit cellHit, 500, cellLayer))
            {
                var cellView = cellHit.collider.GetComponent<CellView>();
                if (cellView != null && cellView.Model.HasUnit)
                {
                    var unitModel = cellView.Model.Unit;
                    unitInfoView.Setup(unitModel);
                }
                else
                {
                    Clear();
                }
            }
        }

        private void Clear()
        {
            unitInfoView.Clear();
        }
    }
}
