using System;
using System.Linq;
using MADP.Controllers;
using MADP.Models;
using MADP.Services.Inventory.Interfaces;
using MADP.Settings;
using UnityEngine;

namespace MADP.Tutorial
{
    public class TutorialStage5_Shop : TutorialStageBase
    {
        private enum Step { Intro, ShopOpen, BuyItem, EquipItem, Finished }
        private Step _currentStep = Step.Intro;
        private ShoppingPhaseController _shopCtrl;
        private IItemService _itemService;

        public override void Enter(Action onCompleted)
        {
            OnCompleted = onCompleted;
            _currentStep = Step.Intro;
            _shopCtrl = TurnCtrl.GetShoppingController();
            _itemService = TurnCtrl.GetItemService();

            LockInput();
            UI.ShowDialogue("Cửa hàng xuất hiện tự động sau mỗi 5 vòng! Đây là nơi giúp quân cờ của ngươi mạnh mẽ hơn.", () => 
            {
                _currentStep = Step.ShopOpen;
                UI.ShowDialogue("Bây giờ ta sẽ mở cửa hàng. Hãy dùng vàng dư dả của ngươi để mua ít nhất một trang bị.", () => 
                {
                    _shopCtrl.OnItemPurchased += HandleItemBought;
                    _itemService.OnItemEquipped += HandleItemEquipped;
                    TurnCtrl.SwitchState(TurnState.Shopping);
                    
                    // Chỉ vào vật phẩm đầu tiên trong shop
                    UI.ShowPointerAtUI(TurnCtrl.GetShopItemRect(0));
                });
            });
        }

        private void HandleItemBought(ItemDataSO item)
        {
            if (_currentStep == Step.ShopOpen)
            {
                _currentStep = Step.BuyItem;
                UI.ShowDialogue($"Ngươi đã mua {item.ItemName}! Bây giờ ta sẽ đóng cửa hàng.", () => 
                {
                    _shopCtrl.ForceCloseShop();
                    UI.ShowDialogue("Để trang bị, hay CLICK VÀO BIỂU TƯỢNG SỐ VÀNG ở dưới cùng màn hình để mở túi đồ của bạn.", () => 
                    {
                        // Chỉ vào icon vàng để mở túi đồ
                        UI.ShowPointerAtUI(TurnCtrl.GetGoldIconRect(TeamColor.Red));
                        UI.ShowDialogue("Sau đó, hãy kéo vật phẩm vừa mua và thả vào một quân cờ Red trên bàn để sử dụng.", () => 
                        {
                            UI.HidePointer();
                            _currentStep = Step.EquipItem;
                            UnlockInput();
                        });
                    });
                });
            }
        }

        private void HandleItemEquipped(UnitModel unit, ItemDataSO item)
        {
            if (_currentStep == Step.EquipItem)
            {
                _currentStep = Step.Finished;
                UI.ShowDialogue($"Tuyệt vời! Quân cờ đã được tăng sức mạnh. Ngươi đã nắm vững mọi quy tắc cơ bản.", () => 
                {
                    UI.ShowDialogue("🎉 CHÚC MỪNG! Ngươi đã hoàn thành toàn bộ hướng dẫn.", () => 
                    {
                        UI.ShowDialogue("Giờ hãy bước vào trận chiến thực sự... và chứng minh bản thân!", () => 
                        {
                            OnCompleted?.Invoke();
                        });
                    });
                });
            }
        }

        public override void Exit()
        {
            if (_shopCtrl != null)
                _shopCtrl.OnItemPurchased -= HandleItemBought;
            
            if (_itemService != null)
                _itemService.OnItemEquipped -= HandleItemEquipped;
        }
    }
}
