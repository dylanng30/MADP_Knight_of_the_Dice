using MADP.Models.UnitActions;
using UnityEngine;

namespace MADP.Systems
{
    public class CellEventSystem : MonoBehaviour
    {
        [SerializeField] private int HealAmount;
        [SerializeField] private int HealRate;
        private void OnEnable()
        {
            ActionSystem.SubscribeReaction<MoveUA>(CheckTileEvent, UnitActionEventType.POST);
        }

        private void OnDisable()
        {
            ActionSystem.UnsubscribeReaction<MoveUA>(CheckTileEvent, UnitActionEventType.POST);
        }

        private void CheckTileEvent(MoveUA moveUA)
        {
            //Lấy cellType
            
            //CellAttribute == RED
            
            
            //CellType == Green
            //Heal 1 lượng nhất định
            //Heal tỉ lệ
            //HealUA healUa = new HealUA();
        }
    }
}