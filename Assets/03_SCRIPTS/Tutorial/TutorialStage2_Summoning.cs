using System;
using MADP.Models;

namespace MADP.Tutorial
{
    public class TutorialStage2_Summoning : TutorialStageBase
    {
        private enum Step { Intro, DiceRoll6, SpawnUnit, BonusTurn, Finished }
        private Step _currentStep = Step.Intro;
        private bool _hasRolled = false;

        public override void Enter(Action onCompleted)
        {
            OnCompleted = onCompleted;
            _currentStep = Step.Intro;
            LockInput();
            UI.ShowDialogue("Bàn cờ đang trống rỗng. Mỗi Hiệp Sĩ cần Vàng và sự may mắn (số 6) để xuất trận.", () => 
            {
                _currentStep = Step.DiceRoll6;
                UI.ShowDialogue("Hãy thử vận may. Ta sẽ ép xúc xắc ra 6 để ngươi có quyền xuất quân!", () => 
                {
                    ForceRoll(6);
                    UnlockInput();
                    UI.ShowPointerAtUI(TurnCtrl.GetRollButtonRect());
                });
            });
        }

        public override void OnDiceRolled(int value)
        {
            if (value > 0) _hasRolled = true;
            if (_currentStep == Step.DiceRoll6 && value == 6)
            {
                UI.HidePointer();
                _currentStep = Step.SpawnUnit;
                UI.ShowDialogue("Tuyệt vời! Số 6! Bây giờ nút chọn quân đã sáng lên.\nNgươi có đủ vàng, hãy chọn quân rẻ nhất để ra trận.", () => 
                {
                    var playerUnit = BoardCtrl.GetAllUnitsByColor(TeamColor.Red)[0];
                    UI.ShowPointerAtUI(TurnCtrl.GetUnitCardRect(playerUnit.Id));
                });
            }
            else if (_currentStep == Step.BonusTurn)
            {
                UI.HidePointer();
            }
        }

        public override void OnTurnCompleted()
        {
            UnityEngine.Debug.Log($"[Tutorial] Stage 2 OnTurnCompleted called. Step: {_currentStep}, HasRolled: {_hasRolled}");

            if (_currentStep == Step.SpawnUnit)
            {
                _hasRolled = false; // Reset dự phòng
                _currentStep = Step.BonusTurn;
                UI.ShowDialogue("Đổ được 6 không chỉ giúp xuất quân mà còn tặng thêm cho ngươi 1 lượt nữa!", () => 
                {
                    UI.ShowDialogue("Hãy gieo xúc xắc một lần nữa và điều khiển quân cờ vừa xuất hiện của ngươi.", () => 
                    {
                        ForceRoll(3);
                        UnlockInput();
                        UI.ShowPointerAtUI(TurnCtrl.GetRollButtonRect());
                    });
                });
            }
            else if (_currentStep == Step.BonusTurn)
            {
                if (!_hasRolled)
                {
                    UnityEngine.Debug.LogWarning("[Tutorial] Stage 2 OnTurnCompleted (BonusTurn) triggered without a roll. Skipping logic.");
                    return;
                }

                _hasRolled = false;
                _currentStep = Step.Finished;
                OnCompleted?.Invoke();
            }
        }

        public override void Exit()
        {
        }
    }
}
