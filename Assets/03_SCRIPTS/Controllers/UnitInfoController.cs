using MADP.Models;
using MADP.Settings;
using MADP.Views;
using MADP.Views.UnitInfo;
using UnityEngine;

namespace MADP.Controllers
{
    public class UnitInfoController : MonoBehaviour
    {
        [SerializeField] private UnitInfoView unitInfoView;
        [SerializeField] private UnitAvatarDatabaseSO unitAvatarDB;
        
        //Test
        [SerializeField] private ItemDataSO testItemSO;
        private UnitModel _selectedUnit;
        
        private void Awake()
        {
            unitInfoView.HideAction += Clear;
            unitInfoView.Clear();
        }
        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                OnMouseDown();
            }
            
            if (Input.GetKeyDown(KeyCode.I) && _selectedUnit != null) {
                _selectedUnit.Inventory.AddItem(testItemSO);
            }
        }

        private void OnMouseDown()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit cellHit, 500))
            {
                if (cellHit.collider != null)
                {
                    var cellView = cellHit.collider.GetComponent<CellView>();
                    if (cellView != null && cellView.Model.HasUnit)
                    {
                        _selectedUnit = cellView.Model.Unit;
                        Sprite avatar = unitAvatarDB.GetAvatar(_selectedUnit.TeamOwner, _selectedUnit.Id);
                        unitInfoView.Show(_selectedUnit, avatar);
                        return;
                    }
                }
            }
            Clear();
        }

        private void Clear()
        {
            _selectedUnit = null;
            unitInfoView.Clear();
        }
    }
}
