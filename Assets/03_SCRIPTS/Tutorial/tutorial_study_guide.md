# Hướng Dẫn Nghiên Cứu Hệ Thống Tutorial (Knight of the Dice)

Tài liệu này giải thích kiến trúc và cách hoạt động của hệ thống hướng dẫn trong game, giúp bạn dễ dàng đọc hiểu code và mở rộng trong tương lai.

## 1. Kiến Trúc Tổng Quan (Architecture)

Hệ thống được thiết kế theo dạng **State Pattern** kết hợp với **Director Pattern**.

### Các Thành Phần Chính:
- **[TutorialDirector.cs](file:///d:/DADPT/Assets/03_SCRIPTS/Tutorial/TutorialDirector.cs)**: "Bộ não" của toàn hệ thống. Nó quản lý danh sách các Stage, xử lý việc chuyển màn và lưu tiến trình người chơi.
- **[TutorialStageBase.cs](file:///d:/DADPT/Assets/03_SCRIPTS/Tutorial/TutorialStageBase.cs)**: Lớp cha trừu tượng (abstract). Mọi màn hướng dẫn đều phải kế thừa từ đây. Nó cung cấp các công cụ: [LockInput](file:///d:/DADPT/Assets/03_SCRIPTS/Tutorial/TutorialStageBase.cs#40-42), [ForceRoll](file:///d:/DADPT/Assets/03_SCRIPTS/Tutorial/TutorialStageBase.cs#43-44), [ShowDialogue](file:///d:/DADPT/Assets/03_SCRIPTS/Tutorial/TutorialUIOverlay.cs#44-51).
- **[TutorialMapInitializer.cs](file:///d:/DADPT/Assets/03_SCRIPTS/Tutorial/TutorialMapInitializer.cs)**: Chịu trách nhiệm chuẩn bị "sân khấu". Nó dọn dẹp bàn cờ và đặt quân, set vàng, set ô đặc biệt trước khi mỗi Stage bắt đầu.
- **[TutorialUIOverlay.cs](file:///d:/DADPT/Assets/03_SCRIPTS/Tutorial/TutorialUIOverlay.cs)**: Quản lý phần hiển thị: Hội thoại (Dialogue) và Con trỏ chỉ dẫn (Pointer).

---

## 2. Luồng Hoạt Động (Flow)

Mỗi màn hướng dẫn (Stage) hoạt động theo chu kỳ sau:

1. **Setup**: [MapInitializer](file:///d:/DADPT/Assets/03_SCRIPTS/Tutorial/TutorialMapInitializer.cs#15-192) chuẩn bị bàn cờ.
2. **Enter**: Stage bắt đầu, gọi hàm [Enter()](file:///d:/DADPT/Assets/03_SCRIPTS/Tutorial/TutorialStage4_SpecialCells.cs#12-32). Thường sẽ hiển thị lời chào đầu tiên.
3. **Execution**: Stage chờ các sự kiện từ Game Logic thông qua:
    - [OnDiceRolled(value)](file:///d:/DADPT/Assets/03_SCRIPTS/Tutorial/TutorialStage2_Summoning.cs#29-47): Khi người chơi gieo xúc xắc.
    - [OnTurnCompleted()](file:///d:/DADPT/Assets/03_SCRIPTS/Tutorial/TutorialStage1_Awakening.cs#53-82): Khi người chơi kết thúc lượt đi.
4. **Transition**: Khi Stage hoàn thành, nó gọi callback `OnCompleted` để [Director](file:///d:/DADPT/Assets/03_SCRIPTS/Tutorial/TutorialDirector.cs#13-136) chuyển sang màn tiếp theo.

---

## 3. Hệ Thống Chỉ Dẫn (Pointer System)

Hệ thống này giúp người chơi biết phải nhấn vào đâu.

- **Pointer tại UI**: Dùng `UI.ShowPointerAtUI(RectTransform rect)`. 
- **Pointer tại Thế giới (World)**: Dùng `UI.ShowPointerAtWorld(Vector3 pos)`.

Để làm được việc này, các Controller chính ([TurnController](file:///d:/DADPT/Assets/03_SCRIPTS/Controllers/TurnController.cs#25-481), [GoldUIManager](file:///d:/DADPT/Assets/03_SCRIPTS/Managers/GoldUIManager.cs#15-192), ...) đã được thêm các hàm "Facade" (cổng giao tiếp) để trả về tọa độ chính xác của các thành phần UI (như nút Roll, thẻ bài, icon vàng).

---

## 4. Cách Đọc Một Script Tutorial Stage

Hãy lấy **[TutorialStage1_Awakening.cs](file:///d:/DADPT/Assets/03_SCRIPTS/Tutorial/TutorialStage1_Awakening.cs)** làm ví dụ:

- **Enum Step**: Chia Stage thành các bước nhỏ (như `DiceRoll3`, [Move](file:///d:/DADPT/Assets/03_SCRIPTS/Controllers/BoardController.cs#126-175), `Finished`). Điều này giúp quản lý logic tuần tự dễ dàng hơn.
- **Switch/If-Else trong OnTurnCompleted**: Đây là nơi kiểm tra xem người chơi đã thực hiện xong hành động yêu cầu chưa để chuyển `Step`.
- **Callback-based Dialogues**: `UI.ShowDialogue("nội dung", () => { ... })`. Code bên trong dấu `{ ... }` sẽ chạy sau khi người chơi nhấn qua đoạn hội thoại.

---

## 5. Mẹo Nghiên Cứu

1. **Theo dấu Pointer**: Xem cách [TurnController](file:///d:/DADPT/Assets/03_SCRIPTS/Controllers/TurnController.cs#25-481) lấy tọa độ từ các View khác nhau. Đây là cách tốt nhất để hiểu mối quan hệ giữa UI và Logic.
2. **Thử nghiệm MapInitializer**: Thay đổi vị trí spawn quân hoặc loại ô đặc biệt trong `SetupStageX` để thấy sự thay đổi.
3. **Mở rộng**: Thử tạo một `TutorialStage6` mới kế thừa [TutorialStageBase](file:///d:/DADPT/Assets/03_SCRIPTS/Tutorial/TutorialStageBase.cs#10-55) và đăng ký nó trong [TutorialDirector](file:///d:/DADPT/Assets/03_SCRIPTS/Tutorial/TutorialDirector.cs#13-136).

---

> [!TIP]
> Luôn sử dụng [LockInput()](file:///d:/DADPT/Assets/03_SCRIPTS/Tutorial/TutorialStageBase.cs#40-42) khi đang hiển thị hội thoại quan trọng để tránh người chơi thực hiện các hành động làm phá vỡ logic hướng dẫn đã định sẵn.
