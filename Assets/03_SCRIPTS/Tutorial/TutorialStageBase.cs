using System;
using MADP.Controllers;

namespace MADP.Tutorial
{
    // Lớp cơ sở (abstract) cho tất cả các màn hướng dẫn.
    // Cung cấp các công cụ: khóa/mở input, ép xúc xắc, và truy cập vào các Controller.
    public abstract class TutorialStageBase
    {
        protected TurnController TurnCtrl;
        protected BoardController BoardCtrl;
        protected TutorialUIOverlay UI;
        protected TutorialMapInitializer MapInit;
        protected Action OnCompleted;

        // Khởi tạo Stage với các tham chiếu cần thiết từ TutorialDirector.
        public virtual void Initialize(TurnController turnCtrl, BoardController boardCtrl, TutorialUIOverlay ui, TutorialMapInitializer mapInit)
        {
            TurnCtrl = turnCtrl;
            BoardCtrl = boardCtrl;
            UI = ui;
            MapInit = mapInit;
        }

        // Gọi khi bắt đầu màn hướng dẫn này.
        /// <param name="onCompleted">Callback gọi khi màn hoàn thành.</param>
        public abstract void Enter(Action onCompleted);

        // Gọi khi thoát khỏi màn hướng dẫn (cleanup).
        public abstract void Exit();

        // --- Helper Methods ---
        protected void LockInput() => TurnCtrl.SetInputLocked(true);
        protected void UnlockInput() => TurnCtrl.SetInputLocked(false);
        protected void ForceRoll(int value) => TurnCtrl.ForceNextDiceValue(value);

        // Xử lý logic khi một lượt chơi vừa kết thúc.
        public virtual void OnTurnCompleted() { }

        // Xử lý logic khi xúc xắc vừa được gieo.
        public virtual void OnDiceRolled(int value) { }
    }
}
