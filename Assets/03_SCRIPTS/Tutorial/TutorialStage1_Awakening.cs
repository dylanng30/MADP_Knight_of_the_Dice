using System;
using MADP.Models;
using UnityEngine;

namespace MADP.Tutorial
{
    public class TutorialStage1_Awakening : TutorialStageBase
    {
        // Các bước nhỏ trong Stage 1
        private enum Step { Intro, DiceRoll3, Move, BlockedIntro, DiceRoll4, GoalExplain, Finished }
        private Step _currentStep = Step.Intro;

        public override void Enter(Action onCompleted)
        {
            OnCompleted = onCompleted;
            _currentStep = Step.Intro;
            StartStage();
        }

        private void StartStage()
        {
            LockInput(); // Khóa input để ngăn người chơi bấm lung tung lúc đang intro
            UI.ShowDialogue("Chào mừng linh hồn nhỏ bé! Ta là Game Master.\nMuốn thoát khỏi đây? Hãy đưa 4 quân cờ của ngươi về Đích ở trung tâm... nếu ngươi sống sót.", () => 
            {
                _currentStep = Step.DiceRoll3;
                UI.ShowDialogue("Đầu tiên, hãy gieo xúc xắc để bắt đầu lượt đi của ngươi.", () => 
                {
                    ForceRoll(3); // Ép xúc xắc ra 3
                    UnlockInput();
                    UI.ShowPointerAtUI(TurnCtrl.GetRollButtonRect()); // Chỉ vào nút Roll
                });
            });
        }

        public override void OnDiceRolled(int value)
        {
            if (_currentStep == Step.DiceRoll3 && value == 3)
            {
                UI.HidePointer();
                _currentStep = Step.Move;
                UI.ShowMessage("Xúc xắc ra 3! Hãy chọn quân cờ và di chuyển 3 bước.", 2f);
                
                // Trỏ vào quân cờ Red đầu tiên để hướng dẫn di chuyển
                var playerUnit = BoardCtrl.GetAllUnitsByColor(TeamColor.Red)[0];
                UI.ShowPointerAtWorld(BoardCtrl.GetUnitView(playerUnit).transform.position);
            }
            else if (_currentStep == Step.DiceRoll4 && value == 4)
            {
                UI.HidePointer();
            }
        }

        public override void OnTurnCompleted()
        {
            if (_currentStep == Step.Move)
            {
                _currentStep = Step.BlockedIntro;
                UI.ShowDialogue("Đôi khi đường đi sẽ bị chặn bởi kẻ thù hoặc đồng đội.", () => 
                {
                    _currentStep = Step.DiceRoll4;
                    UI.ShowDialogue("Hãy gieo xúc xắc lần nữa. Ta sẽ ép ra 4, nhưng ngươi sẽ không thể đi đâu vì có kẻ chặn đường.", () => 
                    {
                        ForceRoll(4);
                        UnlockInput();
                        UI.ShowPointerAtUI(TurnCtrl.GetRollButtonRect());
                    });
                });
            }
            else if (_currentStep == Step.DiceRoll4)
            {
                UI.ShowDialogue("Đường bị chặn! Ngươi nhận được +1 Vàng như một sự bù đắp nhỏ nhoi.", () => 
                {
                    _currentStep = Step.GoalExplain;
                    UI.ShowDialogue("Mục tiêu của ngươi là đưa cả 4 quân về đích. Ai nhanh chân hơn sẽ thắng!", () => 
                    {
                        _currentStep = Step.Finished;
                        OnCompleted?.Invoke();
                    });
                });
            }
        }

        public override void Exit()
        {
        }
    }
}
