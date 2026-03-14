using System;
using System.Collections.Generic;
using MADP.Models;
using MADP.Views.Unit;

namespace MADP.Tutorial
{
    public class TutorialStage3_Combat : TutorialStageBase
    {
        private enum Step { Intro, StatsCheck, InspectUnit, AttackFail, BackKick, Finished }
        private Step _currentStep = Step.Intro;
        private bool _hasRolled = false;

        public override void Enter(Action onCompleted)
        {
            OnCompleted = onCompleted;
            _currentStep = Step.Intro;
            LockInput();
            UI.ShowDialogue("Binh pháp có câu: Biết người biết ta, trăm trận trăm thắng. Hãy nhìn vào quân ta và địch.", () => 
            {
                _currentStep = Step.StatsCheck;
                var blueUnits = BoardCtrl.GetAllUnitsByColor(MADP.Models.TeamColor.Blue);
                var redUnits = BoardCtrl.GetAllUnitsByColor(MADP.Models.TeamColor.Red);

                if (redUnits.Count > 0)
                {
                    var view = BoardCtrl.GetUnitView(redUnits[0]);
                    if (view != null) UI.ShowPointerAtWorld(view.transform.position);
                }

                UI.ShowDialogue("🩸 Máu (đỏ) — về 0 là tử trận.\n🛡️ Khiên (xanh) — giảm sát thương nhận và hồi 1 mỗi lượt.", () => 
                {
                    UI.HidePointer();
                    if (blueUnits.Count > 0)
                    {
                        var view = BoardCtrl.GetUnitView(blueUnits[0]);
                        if (view != null) UI.ShowPointerAtWorld(view.transform.position);
                    }
                    UI.ShowDialogue("Kẻ địch cũng có các số tương tự. Hãy chú ý trước khi tấn công!", () => 
                    {
                        UI.HidePointer();
                        _currentStep = Step.InspectUnit;
                        UI.ShowDialogue("Để xem chi tiết hơn, hãy thử Click Chuột Phải vào quân cờ bất kỳ.", () => 
                        {
                            UI.ShowMessage("Click Chuột Phải vào quân cờ để xem thông tin!", 4f);
                            _currentStep = Step.AttackFail;
                            UI.ShowDialogue("Bây giờ, hãy gieo 4 và tấn công kẻ địch phía trước. Hắn rất khỏe, quân ta sẽ bị đánh bật lại.", () => 
                            {
                                ForceRoll(4);
                                UnlockInput();
                                UI.ShowPointerAtUI(TurnCtrl.GetRollButtonRect());
                            });
                        });
                    });
                });
            });
        }

        public override void OnDiceRolled(int value)
        {
            if (value > 0) _hasRolled = true;
            UI.HidePointer();
            var blueUnits = BoardCtrl.GetAllUnitsByColor(MADP.Models.TeamColor.Blue);

            if (_currentStep == Step.AttackFail && value == 4)
            {
                UI.ShowMessage("Hãy click vào Địch phía trước để tấn công!", 3f);
                if (blueUnits.Count > 0)
                {
                    var view = BoardCtrl.GetUnitView(blueUnits[0]);
                    if (view != null) UI.ShowPointerAtWorld(view.transform.position);
                }
            }
            else if (_currentStep == Step.BackKick && value == 3)
            {
                UI.ShowMessage("Click kẻ địch phía sau để hạ gục hắn bằng Đá Hậu!", 3f);
                if (blueUnits.Count > 1)
                {
                    var view = BoardCtrl.GetUnitView(blueUnits[1]);
                    if (view != null) UI.ShowPointerAtWorld(view.transform.position);
                }
            }
        }

        public override void OnTurnCompleted()
        {
            UnityEngine.Debug.Log($"[Tutorial] Stage 3 OnTurnCompleted called. Step: {_currentStep}, HasRolled: {_hasRolled}");
            
            if (!_hasRolled)
            {
                UnityEngine.Debug.LogWarning("[Tutorial] OnTurnCompleted triggered without a roll. Skipping logic.");
                return;
            }

            if (_currentStep == Step.AttackFail)
            {
                _hasRolled = false;
                UI.ShowDialogue("Tấn công không thành công! Ngươi phải quay về vị trí cũ.", () => 
                {
                    _currentStep = Step.BackKick;
                    UI.ShowDialogue("Nhưng đừng lo, kẻ phía sau đang rất yếu! Hãy gieo 3 và dùng Đá Hậu để hạ hắn.", () => 
                    {
                        ForceRoll(3);
                        UnlockInput();
                        UI.ShowPointerAtUI(TurnCtrl.GetRollButtonRect());
                    });
                });
            }
            else if (_currentStep == Step.BackKick)
            {
                var blueUnits = BoardCtrl.GetAllUnitsByColor(MADP.Models.TeamColor.Blue);
                // Nếu vẫn còn ít nhất 2 địch (con thứ 2 chưa chết) thì coi như chưa đá xong?
                // Tuy nhiên combat result có thể chưa cập nhật ngay. 
                // Ta dựa vào việc đã Roll thành công cho bước này.
                
                _hasRolled = false;
                _currentStep = Step.Finished;
                UI.ShowDialogue("Tuyệt vời! Đá Hậu vừa giúp ngươi tiêu diệt địch vừa kiếm thêm Vàng.", () => 
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
