using System;
using MADP.Models;

namespace MADP.Tutorial
{
    public class TutorialStage4_SpecialCells : TutorialStageBase
    {
        private enum Step { Intro, Heal, Gold, Myth, Harm, Finished }
        private Step _currentStep = Step.Intro;
        private bool _hasRolled = false;

        public override void Enter(Action onCompleted)
        {
            OnCompleted = onCompleted;
            _currentStep = Step.Intro;
            LockInput();
            UI.ShowDialogue("Bàn cờ có những ô màu sắc bí ẩn, mỗi loại mang lại một hiệu ứng khác nhau. Hãy cùng khám phá!", () => 
            {
                _currentStep = Step.Heal;
                UI.ShowDialogue("Đầu tiên là ô xanh lá. Nó sẽ giúp Hiệp Sĩ của ngươi hồi máu.", () => 
                {
                    ForceRoll(2);
                    UnlockInput();
                    
                    // Chỉ vào ô Heal
                    var spawnCell = BoardCtrl.GetSpawnCell(TeamColor.Red);
                    int baseIdx = spawnCell != null ? spawnCell.Index : 0;
                    UI.ShowPointerAtWorld(TurnCtrl.GetCellWorldPos((baseIdx + 6) % BoardCtrl.Board.AroundCells.Count));
                });
            });
        }

        public override void OnDiceRolled(int value)
        {
            if (value > 0) _hasRolled = true;
            if (_currentStep == Step.Heal && value == 2) 
            {
                UI.HidePointer();
                UI.ShowMessage("Di chuyển vào ô Xanh để hồi máu!", 2f);
            }
            else if (_currentStep == Step.Gold && value == 2) 
            {
                UI.HidePointer();
                UI.ShowMessage("Di chuyển vào ô Vàng để nhận thêm vàng!", 2f);
            }
            else if (_currentStep == Step.Myth && value == 2) 
            {
                UI.HidePointer();
                UI.ShowMessage("Di chuyển vào ô Tím để xem điều bất ngờ!", 2f);
            }
            else if (_currentStep == Step.Harm && value == 2) 
            {
                UI.HidePointer();
                UI.ShowMessage("Cẩn thận! Ô Đỏ sẽ gây sát thương.", 2f);
            }
        }

        public override void OnTurnCompleted()
        {
            UnityEngine.Debug.Log($"[Tutorial] Stage 4 OnTurnCompleted called. Step: {_currentStep}, HasRolled: {_hasRolled}");

            if (!_hasRolled)
            {
                UnityEngine.Debug.LogWarning("[Tutorial] Stage 4 OnTurnCompleted triggered without a roll. Skipping logic.");
                return;
            }

            _hasRolled = false;

            if (_currentStep == Step.Heal)
            {
                _currentStep = Step.Gold;
                UI.ShowDialogue("Máu đã được hồi phục! Tiếp theo là ô màu Vàng.", () => 
                {
                    ForceRoll(2);
                    UnlockInput();
                    
                    // Chỉ vào ô Gold
                    var spawnCell = BoardCtrl.GetSpawnCell(TeamColor.Red);
                    int baseIdx = spawnCell != null ? spawnCell.Index : 0;
                    UI.ShowPointerAtWorld(TurnCtrl.GetCellWorldPos((baseIdx + 8) % BoardCtrl.Board.AroundCells.Count));
                });
            }
            else if (_currentStep == Step.Gold)
            {
                _currentStep = Step.Myth;
                UI.ShowDialogue("Tiền vào như nước! Bây giờ hãy thử vận may với ô màu Tím (Bí ẩn).", () => 
                {
                    ForceRoll(2);
                    UnlockInput();
                    
                    // Chỉ vào ô Myth
                    var spawnCell = BoardCtrl.GetSpawnCell(TeamColor.Red);
                    int baseIdx = spawnCell != null ? spawnCell.Index : 0;
                    UI.ShowPointerAtWorld(TurnCtrl.GetCellWorldPos((baseIdx + 10) % BoardCtrl.Board.AroundCells.Count));
                });
            }
            else if (_currentStep == Step.Myth)
            {
                _currentStep = Step.Harm;
                UI.ShowDialogue("Ô Tím thật thú vị đúng không? Cuối cùng là ô màu Đỏ. Hãy cẩn thận vì nó gây sát thương!", () => 
                {
                    ForceRoll(2);
                    UnlockInput();
                    
                    // Chỉ vào ô Harm
                    var spawnCell = BoardCtrl.GetSpawnCell(TeamColor.Red);
                    int baseIdx = spawnCell != null ? spawnCell.Index : 0;
                    UI.ShowPointerAtWorld(TurnCtrl.GetCellWorldPos((baseIdx + 12) % BoardCtrl.Board.AroundCells.Count));
                });
            }
            else if (_currentStep == Step.Harm)
            {
                _currentStep = Step.Finished;
                UI.ShowDialogue("Tất cả các ô đặc biệt đều có thể thay đổi cục diện trận đấu. Hãy tận dụng chúng!", () => 
                {
                    OnCompleted?.Invoke();
                });
            }
        }

        public override void Exit()
        {
        }
    }
}
