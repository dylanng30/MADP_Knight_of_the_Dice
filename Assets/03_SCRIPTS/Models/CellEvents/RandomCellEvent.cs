using MADP.Models.CellEvents.Interfaces;
using MADP.Services.Gold.Interfaces;
using UnityEngine;

namespace MADP.Models.CellEvents
{
    public class RandomCellEvent : ICellEvent
    {
        private readonly IGoldService _goldService;

        public RandomCellEvent(IGoldService goldService)
        {
            _goldService = goldService;
        }
        public bool CanExecute(CellModel cell)
        {
            return cell.Attribute == CellAttribute.Myth;
        }

        public void Execute(UnitModel unit, CellModel cell)
        {
            Debug.Log($"{GetType()}");
            int randomIndex = Random.Range(1, 5);
            switch (randomIndex)
            {
                case 1:
                    //Hàm sự kiện
                    break;
                case 2:
                    //Hàm sự kiện
                    break;
            }
        }
        
        
    }
}