# MADP_Knight_of_the_Dice

🎲 Cờ Cá Ngựa Nhập Vai (RPG Ludo 3D)
Một phiên bản biến thể hiện đại của trò chơi Cờ Cá Ngựa (Ludo/Parcheesi) truyền thống, kết hợp yếu tố nhập vai chiến thuật (RPG) và hệ thống Trí tuệ nhân tạo (AI) phức tạp. Dự án được phát triển trên Unity 3D, tuân thủ nghiêm ngặt mô hình MVC và các nguyên tắc SOLID nhằm đảm bảo mã nguồn sạch (Clean Code), dễ bảo trì và mở rộng.

✨ Tính Năng Nổi Bật
Gameplay Đột Phá (Ludo + RPG): Không chỉ dựa vào nhân phẩm xúc xắc, người chơi cần tính toán chiến thuật qua các hệ thống:

Combat System: Các quân cờ có Máu (HP), Giáp (Shield) và Sát thương. Đá địch không chỉ là "chạm", mà là những pha giao tranh tính toán.

Hệ thống Vai trò (Roles): Quân cờ được chia class (Attacker, Defender, Speller, Miner), mỗi class có thế mạnh riêng.

Ô Sự kiện (Cell Events): Bản đồ chứa các ô tương tác ngẫu nhiên (Hồi máu, Nhận vàng, Bẫy...).

Kinh tế (Gold System): Thu thập vàng trên bản đồ để nâng cấp hoặc sử dụng kỹ năng.

Hệ Thống AI Cấp Cao (Expectimax & Heuristic): Bot không đi ngẫu nhiên. Chúng biết đánh giá bàn cờ, "sợ hãi" khi bị địch ngắm bắn (tính xác suất rủi ro qua Chance Nodes), và "khát máu" khi thấy con mồi yếu.

Hệ thống Lobby Nâng Cao: Cho phép tùy chỉnh phòng chơi:

Thêm/Bớt Bot với các mức độ thông minh khác nhau.

Thay đổi Bản đồ (Classic, Snow, Desert).

Tùy chỉnh thời gian mỗi lượt và mật độ ô sự kiện.

Quản lý chống trùng lặp màu sắc đội (Team Color).

🏗️ Kiến Trúc Hệ Thống & Design Patterns
Dự án được thiết kế chặt chẽ theo các pattern chuẩn mực trong ngành Game Development:

MVC (Model - View - Controller): Phân tách tuyệt đối giữa Dữ liệu (Model), Giao diện (View) và Logic (Controller/Service).

State Machine Pattern: Quản lý luồng theo từng Lượt (Turn) qua các trạng thái độc lập (RollingState, ChoosingState...).

Strategy Pattern: Dùng để quản lý các "bộ não" AI khác nhau (IBotStrategy, SmartBotStrategy) giúp dễ dàng tháo lắp hoặc tạo thêm độ khó cho Bot.

Observer Pattern (Events/Actions): View lắng nghe Model thông qua C# Action và Event, loại bỏ hoàn toàn việc View phải check update liên tục (Update() method).

Singleton Pattern: Áp dụng tiết chế cho các Manager lõi (GameManager, UIManager).

Scriptable Objects (SO): Quản lý toàn bộ thông số tĩnh (Trọng số AI, Bảng màu đội, Setting bản đồ) giúp Game Designer dễ dàng cân bằng game mà không cần mở code.

🧠 Cơ Chế Hoạt Động Của Trí Tuệ Nhân Tạo (AI)
AI trong game sử dụng thuật toán Expectimax (được tối ưu hóa độ sâu) kết hợp với Hàm Đánh Giá (Heuristic Function).
Điểm số của một nước đi được tính theo công thức:

Score = (W1 * Kick) + (W2 * Safe) + (W3 * Home) + (W4 * Distance) - (W5 * Danger)

W1 -> W4: Điểm cộng (Thưởng) khi đạt lợi thế như đá địch, vào ô an toàn, hay tiến gần về đích.

W5 (Danger): Điểm trừ (Phạt) tính toán dựa trên kỳ vọng toán học (Expected Value). Bot quét tất cả kẻ thù xung quanh, tính khoảng cách và nhân với xác suất xúc xắc (1/6) để ra tỉ lệ bị đá ở lượt tiếp theo.

🚀 Hướng Dẫn Cài Đặt (Getting Started)
Yêu cầu: Unity 2022.3 LTS (hoặc mới hơn).

Cài đặt:

Clone repository về máy.

Mở project bằng Unity Hub.

Import thư viện TextMeshPro (nếu có popup yêu cầu).

Bắt đầu: * Mở scene Loading.

Thêm Bot, tùy chỉnh Settings và nhấn Start Game để trải nghiệm!

🤝 Định Hướng Phát Triển Tương Lai
[ ] Hoàn thiện Server/Client Multiplayer (Socket.IO/Mirror).

[ ] Tích hợp hiệu ứng hạt (VFX) và Âm thanh (Audio System).

[ ] Chế độ Huấn luyện AI thông qua Machine Learning (Unity ML-Agents).
