using System.Collections.Generic;
using System.Linq;
using _03_SCRIPTS.MLAgent.Services;
using MADP.Controllers;
using MADP.Models;
using MADP.Models.Inventory;
using MADP.Settings;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace _03_SCRIPTS.MLAgent
{
    public class TeamAgent : Agent
    {
        [Header("References")] [SerializeField]
        private TurnController turnController;

        [SerializeField] private BoardController boardController;
        [Header("Settings")] [SerializeField] private TeamColor teamOwner;
        [SerializeField] private int maxRound = 500;
        [SerializeField] private int maxGold = 50;
        [SerializeField] private int maxItemStatBonus = 10;

        private readonly IDistanceService _distanceService = new DistanceService();
        private readonly int _maxCellsAround = 64; // Số lượng ô xung quanh

        private List<ItemDataSO> _currentShopItems = new();
        private bool _isShoppingPhase = false;

        public override void CollectObservations(VectorSensor sensor)
        {
            BoardModel board = boardController.Board;

            // 1. Xúc xắc - 1 sensor
            int diceValue = turnController.CurrentDiceValue;
            var normalizedDiceValue = diceValue / 6f;
            sensor.AddObservation(normalizedDiceValue);

            // 2. Vòng đấu - 1 sensor
            var currentRound = turnController.CurrentRound;
            var normalizedCurrentRound = (float)currentRound / maxRound;
            sensor.AddObservation(normalizedCurrentRound);

            // 3. Vàng - 4 sensor
            var goldService = turnController.GoldService;

            // Vàng của mình
            var teamGold = goldService.GetGold(teamOwner);
            var normalizedTeamGold = Mathf.Clamp01((float)teamGold / maxGold);
            sensor.AddObservation(normalizedTeamGold);

            // Vàng của 3 đối thủ
            foreach (var slot in turnController.ActiveSlots)
            {
                if (slot.TeamColor != teamOwner)
                {
                    var enemyGold = goldService.GetGold(slot.TeamColor);
                    var normalizedEnemyGold = Mathf.Clamp01((float)enemyGold / maxGold);
                    sensor.AddObservation(normalizedEnemyGold);
                }
            }

            // 4. Túi đồ - 30 sensor
            LobbySlotModel mySlot = turnController.ActiveSlots.FirstOrDefault(s => s.TeamColor == teamOwner);

            if (mySlot != null)
            {
                int maxSlots = PlayerInventoryModel.ItemSlots; // 10
                int currentItems = mySlot.Inventory.Items.Count; // Số lượng item hiện tại

                for (int i = 0; i < maxSlots; i++)
                {
                    if (i < currentItems)
                    {
                        var item = mySlot.Inventory.Items[i];

                        var normalizedBonusHealth = (float)item.BonusHealth / maxItemStatBonus;
                        var normalizedBonusDamage = (float)item.BonusDamage / maxItemStatBonus;
                        var normalizedBonusArmor = (float)item.BonusArmor / maxItemStatBonus;

                        sensor.AddObservation(normalizedBonusHealth);
                        sensor.AddObservation(normalizedBonusDamage);
                        sensor.AddObservation(normalizedBonusArmor);
                    }
                    else
                    {
                        sensor.AddObservation(0f);
                        sensor.AddObservation(0f);
                        sensor.AddObservation(0f);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 30; i++) sensor.AddObservation(0f);
            }

            // 5. Trạng thái quân cờ - 24 sensor
            List<UnitModel> myUnits = boardController.GetAllUnitsByColor(teamOwner);
            foreach (var unit in myUnits)
            {
                // a. Trạng thái (0: InNest, 0.5: Moving, 1: InHome)
                float stateValue = 0f;
                switch (unit.State)
                {
                    case UnitState.InNest:
                        stateValue = 0f;
                        break;
                    case UnitState.Moving:
                        stateValue = 0.5f;
                        break;
                    case UnitState.InHome:
                        stateValue = 1f;
                        break;
                }

                sensor.AddObservation(stateValue);

                // b. Máu (%)
                var normalizedHealth = (float)unit.CurrentHealth / unit.MaxHealth;
                sensor.AddObservation(normalizedHealth);

                // c. Khoảng cách tới đích (%) - Tính theo DistanceService
                int distanceToGate = _distanceService.CalculateDistanceToGate(unit, board);

                // Khoảng cách càng nhỏ thì `% đi được` càng lớn (1.0 = Về đến cổng, 0 = Mới đẻ ra)
                float progress;
                if (unit.State == UnitState.InHome) progress = 1f;
                else if (unit.State == UnitState.InNest) progress = 0f;
                else
                {
                    progress = Mathf.Clamp01((float)(_maxCellsAround - distanceToGate) / _maxCellsAround);
                }

                sensor.AddObservation(progress);

                // d. Mối đe dọa (Threat)
                sensor.AddObservation(CalculateThreat(unit, boardController));

                // e. Cơ hội ăn mạng (Kill Op)
                sensor.AddObservation(CalculateKillOpportunity(unit, boardController, diceValue));

                // f. Trang bị (Đang có đồ buff không, quy đổi ra 1 số tổng)
                var normalizedBonusHealth = (float)unit.Inventory.GetTotalBonusHealth() / maxItemStatBonus;
                var normalizedBonusDamage = (float)unit.Inventory.GetTotalBonusDamage() / maxItemStatBonus;
                var normalizedBonusArmor = (float)unit.Inventory.GetTotalBonusArmor() / maxItemStatBonus;
                float buffScore = normalizedBonusHealth + normalizedBonusDamage + normalizedBonusArmor;
                sensor.AddObservation(Mathf.Clamp01(buffScore));
            }

            // 6. Bàn cờ - 384 sensor
            for (int i = 0; i < _maxCellsAround; i++)
            {
                CellModel cell = board.AroundCells[i];

                // Có quân cờ không?
                int hasUnit = cell.HasUnit ? 1 : 0;
                sensor.AddObservation(hasUnit);

                if (cell.HasUnit)
                {
                    // Quân cờ thuộc đội mình hay không?
                    var isMyTeam = cell.Unit != null && cell.Unit.TeamOwner == teamOwner ? 1f : 0f;
                    sensor.AddObservation(isMyTeam);

                    if (cell.Unit != null)
                    {
                        var normalizedHealth = (float)cell.Unit.CurrentHealth / cell.Unit.MaxHealth;
                        sensor.AddObservation(normalizedHealth);
                    }
                    else
                    {
                        sensor.AddObservation(0f);
                    }
                }
                else
                {
                    sensor.AddObservation(-1f);
                    sensor.AddObservation(0f);
                }

                bool isMySpawn = cell.Structure == CellStructure.Spawn && cell.TeamOwner == teamOwner;
                bool isMyGate = cell.Structure == CellStructure.Gate && cell.TeamOwner == teamOwner;
                sensor.AddObservation(isMySpawn ? 1f : 0f);
                sensor.AddObservation(isMyGate ? 1f : 0f);

                // Thuộc tính hiệu ứng đặc biệt của Ô Cờ
                float attrValue = 0f;
                switch (cell.Attribute)
                {
                    case CellAttribute.Harm: attrValue = -1f; break;
                    case CellAttribute.Heal: attrValue = 1f; break;
                    case CellAttribute.Gold: attrValue = 0.5f; break;
                    case CellAttribute.Myth: attrValue = 0.66f; break;
                }

                sensor.AddObservation(attrValue);
            }

            // 7. Cửa hàng - 12 sensor (3 items * 4 features: Health, Damage, Armor, Price)
            for (int i = 0; i < 3; i++)
            {
                if (i < _currentShopItems.Count && _currentShopItems[i] != null)
                {
                    var item = _currentShopItems[i];
                    sensor.AddObservation((float)item.BonusHealth / maxItemStatBonus);
                    sensor.AddObservation((float)item.BonusDamage / maxItemStatBonus);
                    sensor.AddObservation((float)item.BonusArmor / maxItemStatBonus);
                    sensor.AddObservation(Mathf.Clamp01((float)item.Price / maxGold));
                }
                else
                {
                    sensor.AddObservation(0f);
                    sensor.AddObservation(0f);
                    sensor.AddObservation(0f);
                    sensor.AddObservation(0f);
                }
            }
        }

        // --- HELPER METHODS ---
        private float CalculateThreat(UnitModel unit, BoardController boardCtrl)
        {
            var currentCell = boardCtrl.GetCurrentCellOfUnit(unit);
            if (currentCell == null) return 0f;

            int myIndex = boardCtrl.Board.AroundCells.IndexOf(currentCell);
            int totalCells = boardCtrl.Board.AroundCells.Count;
            float maxThreat = 0f;

            // Kiểm tra 6 ô xung quanh
            for (int i = 1; i <= 6; i++)
            {
                // Kiểm tra phía sau (bị đối thủ đuổi theo đá tới)
                int checkIndexBehind = (myIndex - i + totalCells) % totalCells;
                CellModel cellBehind = boardCtrl.Board.AroundCells[checkIndexBehind];
                if (cellBehind != null && cellBehind.HasUnit && cellBehind.Unit != null &&
                    cellBehind.Unit.TeamOwner != teamOwner)
                {
                    float threat = (7f - i) / 6f;
                    if (threat > maxThreat) maxThreat = threat;
                }

                // Kiểm tra phía trước (chạy tới trúng đối thủ đang đứng đợi, hoặc cơ chế đá ngược)
                int checkIndexAhead = (myIndex + i) % totalCells;
                CellModel cellAhead = boardCtrl.Board.AroundCells[checkIndexAhead];
                if (cellAhead != null && cellAhead.HasUnit && cellAhead.Unit != null &&
                    cellAhead.Unit.TeamOwner != teamOwner)
                {
                    float threat = (7f - i) / 6f;
                    if (threat > maxThreat) maxThreat = threat;
                }
            }

            return maxThreat;
        }

        private float CalculateKillOpportunity(UnitModel unit, BoardController boardCtrl, int diceValue)
        {
            var currentCell = boardCtrl.GetCurrentCellOfUnit(unit);
            if (currentCell == null) return 0f;

            int myIndex = boardCtrl.Board.AroundCells.IndexOf(currentCell);
            int totalCells = boardCtrl.Board.AroundCells.Count;

            // Kiểm tra phía trước
            int checkIndexAhead = (myIndex + diceValue) % totalCells;
            CellModel cellAhead = boardCtrl.Board.AroundCells[checkIndexAhead];
            if (cellAhead != null && cellAhead.HasUnit && cellAhead.Unit != null &&
                cellAhead.Unit.TeamOwner != teamOwner)
            {
                return 1f; // Mồi ngon phía trước
            }

            // Kiểm tra phía sau
            int checkIndexBehind = (myIndex - diceValue + totalCells) % totalCells;
            CellModel cellBehind = boardCtrl.Board.AroundCells[checkIndexBehind];
            if (cellBehind != null && cellBehind.HasUnit && cellBehind.Unit != null &&
                cellBehind.Unit.TeamOwner != teamOwner)
            {
                return 1f; // Mồi ngon phía sau
            }

            return 0f;
        }

        public override void Initialize()
        {
            // Kết nối các Event Thưởng Phạt Zero-Sum với Ban điều hành Bàn Cờ (BoardController)
            boardController.OnUnitKilledAnother += HandleUnitKilledAnother;
            boardController.OnUnitEnteredHome += HandleUnitEnteredHome;
            boardController.OnUnitAdvancedInHome += HandleUnitAdvancedInHome;
            boardController.OnGameRanked += HandleGameRanked;

            // Lắng nghe tín hiệu tới lượt từ TurnController
            turnController.OnMLAgentTurn += HandleMLAgentTurn;
            turnController.OnMLAgentShoppingTurn += HandleMLAgentShoppingTurn;
        }

        private void HandleMLAgentTurn(TeamColor turnTeam)
        {
            if (turnTeam == teamOwner)
            {
                _isShoppingPhase = false;
                _currentShopItems.Clear();
                // Đúng lượt của AI này -> Yêu cầu Agent request một hành động
                RequestDecision();
            }
        }

        private void HandleMLAgentShoppingTurn(TeamColor turnTeam, List<ItemDataSO> shopItems)
        {
            if (turnTeam == teamOwner)
            {
                _isShoppingPhase = true;
                _currentShopItems = shopItems;
                RequestDecision();
            }
        }

        public override void OnEpisodeBegin()
        {
            // Được gọi mỗi khi EndEpisode chạy. 
            // Vì Game được Reload bằng SceneManager.LoadScene trong TurnController khi end game, 
            // toàn bộ Object sẽ tự động được khởi tạo lại ở Episode mới nên không cần code reset biến/chỉ số ở đây.
            Debug.Log($"[MLAgent] Bắt đầu Episode mới cho đội {teamOwner}");
            Time.timeScale = 100f; 
            _isShoppingPhase = false;
            _currentShopItems.Clear();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            // Gỡ kết nối để tránh lỗi rò rỉ bộ nhớ
            boardController.OnUnitKilledAnother -= HandleUnitKilledAnother;
            boardController.OnUnitEnteredHome -= HandleUnitEnteredHome;
            boardController.OnUnitAdvancedInHome -= HandleUnitAdvancedInHome;
            boardController.OnGameRanked -= HandleGameRanked;
            turnController.OnMLAgentTurn -= HandleMLAgentTurn;
            turnController.OnMLAgentShoppingTurn -= HandleMLAgentShoppingTurn;
        }

        #region REWARD ZERO-SUM EVENTS

        private void HandleUnitKilledAnother(UnitModel attacker, UnitModel victim)
        {
            if (attacker.TeamOwner == teamOwner)
            {
                AddReward(0.05f); // Thưởng mình vì đã đá bay màu địch
            }
            else if (victim.TeamOwner == teamOwner)
            {
                AddReward(-0.05f); // Phạt mình vì bị địch đá bay màu
            }
        }

        private void HandleUnitEnteredHome(UnitModel unit)
        {
            if (unit.TeamOwner == teamOwner)
            {
                AddReward(0.25f); // Thưởng lết qua Cửa 
            }
        }

        private void HandleUnitAdvancedInHome(UnitModel unit, int homeIndex)
        {
            if (unit.TeamOwner == teamOwner)
            {
                AddReward(0.01f); // Thưởng tự hào bò lên từng bậc 
            }
        }

        private void HandleGameRanked(List<TeamColor> ranking)
        {
            // Tìm xem mình đứng hạng mấy?
            int rankIndex = ranking.IndexOf(teamOwner);
            if (rankIndex == -1) return;

            float reward = 0;
            switch (rankIndex)
            {
                case 0: reward = 1.0f; break; // Hạng 1 (Top 1)
                case 1: reward = 0.35f; break; // Hạng 2
                case 2: reward = -0.35f; break; // Hạng 3
                case 3: reward = -1.0f; break; // Hạng 4 (Bét)
            }

            AddReward(reward);

            // Chốt hạ ván đấu cho ML-Agent
            EndEpisode();
            Debug.Log($"[MLAgent] Team {teamOwner} kết thúc với Hạng {rankIndex + 1}. Reward: {reward}");
        }

        #endregion

        // --- ACTION SPACE ---
        public override void OnActionReceived(ActionBuffers actions)
        {
            if (_isShoppingPhase)
            {
                // Branch 3: Shopping (0: Skip, 1-3: Buy Item Index)
                int shopAction = actions.DiscreteActions[3];
                if (shopAction == 0)
                {
                    _isShoppingPhase = false;
                    _currentShopItems.Clear();
                    return; // End shopping phase for this agent
                }

                int itemIndex = shopAction - 1;
                bool success = turnController.TryBuyItemForAgent(itemIndex);
                if (success)
                {
                    AddReward(0.005f); // Thưởng vì đã mua được đồ
                    RequestDecision(); // Có thể mua tiếp
                }
                else
                {
                    AddReward(-0.001f); // Phạt nhẹ nếu mua lỗi (không đủ tiền, hết chỗ, hoặc item rỗng)
                }
                return;
            }
            
            // Branch 0: Chọn Quân Cờ (0: Skip, 1-4: Unit Index)
            int unitBranch = actions.DiscreteActions[0];
            if (unitBranch == 0)
            {
                AddReward(-1.0f / maxRound); // Phạt lười biếng bỏ lượt
                turnController.EndTurn();
                return;
            }

            // Branch 1: Lệnh Hành Động
            int commandBranch = actions.DiscreteActions[1];
            if (commandBranch == 0)
            {
                AddReward(-1.0f / maxRound); // Lỗi logic hoặc ngưng trệ
                turnController.EndTurn();
                return;
            }

            // Lấy Unit được chọn
            int unitIndex = unitBranch - 1;
            List<UnitModel> myUnits = boardController.GetAllUnitsByColor(teamOwner);
            if (unitIndex >= myUnits.Count || unitIndex < 0)
            {
                // Masking lỗi, fallback để không chết ván đấu
                AddReward(-1.0f / maxRound);
                turnController.EndTurn();
                return;
            }

            UnitModel targetUnit = myUnits[unitIndex];
            int diceValue = turnController.CurrentDiceValue;

            // Trừ điểm Zero-Sum thời gian. Cứ có hành động là mất điểm để ép chốt Game.
            AddReward(-1.0f / maxRound);

            // Xử lý các lệnh Hành Động dựa trên Branch 1
            // Cộng thưởng trực tiếp tại các hành động (Dense Reward được phân loại)
            switch (commandBranch)
            {
                case 1: // Xuất quân
                    AddReward(0.005f);
                    boardController.SpawnUnitForAgent(targetUnit, () => turnController.EndTurn());
                    break;
                case 2: // Đi thẳng
                    AddReward(0.001f);
                    boardController.NormalMoveForwardForAgent(targetUnit, diceValue, () => turnController.EndTurn());
                    break;
                case 3: // Đá tiến
                    AddReward(0.008f); // Đá quân rất đáng khích lệ
                    boardController.AttackForwardForAgent(targetUnit, diceValue, () => turnController.EndTurn());
                    break;
                case 4: // Đá hậu
                    AddReward(0.008f);
                    boardController.AttackBackwardForAgent(targetUnit, diceValue, () => turnController.EndTurn());
                    break;
                case 5: // Nhảy cóc (xúc xắc 1)
                    AddReward(0.002f);
                    boardController.JumpOverNormalMoveForAgent(targetUnit, () => turnController.EndTurn());
                    break;
                case 6: // Lên điểm Cửa chuồng
                    AddReward(0.005f); // Bò về nhà tốt
                    boardController.EnterGateForAgent(targetUnit, diceValue, () => turnController.EndTurn());
                    break;
                case 7: // Di chuyển trong chuồng
                    AddReward(0.004f);
                    boardController.MoveInHomeForAgent(targetUnit, diceValue, () => turnController.EndTurn());
                    break;
                case 8: // Sử dụng Item
                    AddReward(0.002f);
                    int itemSlotBranch = actions.DiscreteActions[2]; // Branch 2: Item Slot
                    HandleItemUsage(targetUnit, itemSlotBranch);
                    break;
                default:
                    turnController.EndTurn();
                    break;
            }
        }

        private void HandleItemUsage(UnitModel unit, int slotIndex)
        {
            var mySlot = turnController.ActiveSlots.FirstOrDefault(s => s.TeamColor == teamOwner);
            if (mySlot == null || mySlot.Inventory == null)
            {
                turnController.EndTurn();
                return;
            }

            if (slotIndex < mySlot.Inventory.Items.Count)
            {
                var itemToUse = mySlot.Inventory.Items[slotIndex];
                bool success = turnController.ItemService.TryEquipItem(mySlot.Inventory, unit, itemToUse);
                if (success)
                {
                    // Item dùng xong, ngầm nhường quyền cho component Decision Requester gọi Action mới
                    Debug.Log($"[MLAgent] Team {teamOwner} đã buff {itemToUse.ItemName} cho {unit.Id}");
                    return;
                }
            }

            // Không thành công thì bỏ lượt chống kẹt
            turnController.EndTurn();
        }

        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            if (_isShoppingPhase)
            {
                // Khóa Branch 0, 1, 2 (Chừa lại action 0 để không bị lỗi "All actions masked")
                for (int i = 1; i <= 4; i++) actionMask.SetActionEnabled(0, i, false);
                for (int i = 1; i <= 8; i++) actionMask.SetActionEnabled(1, i, false);
                for (int i = 1; i <= 9; i++) actionMask.SetActionEnabled(2, i, false);

                // Mở Branch 3 (0: Skip luôn mở, 1-3: Buy nếu có item)
                for (int i = 1; i <= 3; i++)
                {
                    bool itemExists = i - 1 < _currentShopItems.Count && _currentShopItems[i - 1] != null;
                    if (!itemExists) actionMask.SetActionEnabled(3, i, false);
                }
                return;
            }

            // --- NORMAL TURN MASKING ---
            // Khóa Branch 3
            for (int i = 1; i <= 3; i++) actionMask.SetActionEnabled(3, i, false);

            // 1. Kiểm tra Lượt Đi
            // Nếu hiện tại không phải là lượt của Agent này, khóa (disable) toàn bộ các nhánh.
            // Việc này giúp AI nhận biết mình không có quyền thao tác lung tung.
            if (turnController.CurrentTeam != teamOwner || turnController.IsPlayerTurn)
            {
                for (int i = 1; i <= 4; i++) actionMask.SetActionEnabled(0, i, false);
                return;
            }

            // Mặc định, ta sẽ khóa tất cả các Lệnh (Nhánh 1) và Các Món Đồ (Nhánh 2).
            // Chừa lại action 0 (Skip/Default) cho mỗi nhánh để tránh lỗi Unity ML-Agents.
            for (int i = 1; i <= 8; i++) actionMask.SetActionEnabled(1, i, false);
            for (int i = 1; i <= 9; i++) actionMask.SetActionEnabled(2, i, false);

            List<UnitModel> myUnits = boardController.GetAllUnitsByColor(teamOwner);
            int diceValue = turnController.CurrentDiceValue;
            var pathfinding = boardController.PathfindingService;
            var boardModel = boardController.Board;

            // 2. Mở khóa Món Đồ (Branch 2 và nhánh 1 Action 8)
            var mySlot = turnController.ActiveSlots.FirstOrDefault(s => s.TeamColor == teamOwner);
            bool hasItems = false;

            if (mySlot != null && mySlot.Inventory != null && mySlot.Inventory.Items.Count > 0)
            {
                hasItems = true;
                // Có đồ trong túi -> Cho phép sử dụng Hành động "Dùng Đồ" (số 8 trên Branch 1)
                actionMask.SetActionEnabled(1, 8, true);

                // Mở khóa cho AI chọn đúng các Slot đồ trong Balo (Branch 2)
                for (int i = 0; i < mySlot.Inventory.Items.Count; i++)
                {
                    actionMask.SetActionEnabled(2, i, true);
                }
            }

            // 3. Quét từng quân cờ để Mở Khóa các Lệnh Chiến Thuật
            for (int u = 0; u < myUnits.Count; u++)
            {
                UnitModel unit = myUnits[u];
                bool unitCanDoSomething = false; // Cờ đánh dấu để xem quân này có đứng im chịu chết không?

                // A. Hành động Xuất Quân (Spawn) - Action 1
                if (boardController.CanSpawnUnit(unit, diceValue))
                {
                    // Đổ xúc xắc 6 điểm, còn điểm tài nguyên, chuồng trống -> Mở quyền Gọi Đệ
                    actionMask.SetActionEnabled(1, 1, true);
                    unitCanDoSomething = true;
                }

                var currentCell = boardController.GetCurrentCellOfUnit(unit);
                if (currentCell != null && unit.State == UnitState.Moving)
                {
                    // B. Di chuyển lên Cổng chuồng an toàn (Gate) - Action 6
                    if (currentCell.Structure == CellStructure.Gate && currentCell.TeamOwner == unit.TeamOwner)
                    {
                        if (boardModel.HomeCells.TryGetValue(unit.TeamOwner, out var homeCells) &&
                            diceValue - 1 < homeCells.Count)
                        {
                            var targetCell = homeCells[diceValue - 1];
                            // Cần quét mảng để xem đường lên Gate có bị đồng đội đứng cản không
                            bool isBlocked = false;
                            for (int i = 0; i < diceValue; i++)
                            {
                                if (i == diceValue - 1) break;
                                if (homeCells[i].HasUnit) isBlocked = true;
                            }

                            if (!isBlocked && !targetCell.HasUnit)
                            {
                                actionMask.SetActionEnabled(1, 6, true);
                                unitCanDoSomething = true;
                            }
                        }
                    }
                    else
                    {
                        // C. Kỹ năng Nhảy Cóc - Action 5 (Chỉ dùng được khi đổ ra 1 điểm)
                        if (diceValue == 1)
                        {
                            var forwardPathToGate = pathfinding.GetPathToGate(boardModel, currentCell);
                            if (forwardPathToGate.Count > 0 &&
                                !boardController.IsPathBlocked(forwardPathToGate, unit.TeamOwner))
                            {
                                actionMask.SetActionEnabled(1, 5, true);
                                unitCanDoSomething = true;
                            }
                        }

                        // Kiểm tra đường đi Tiến (Forward) mặc định của bàn cờ
                        var forwardPath = pathfinding.GetPath(boardModel, currentCell, diceValue);
                        if (forwardPath.Count > 0 && !boardController.IsPathBlocked(forwardPath, unit.TeamOwner))
                        {
                            var targetCell = forwardPath.Last();
                            if (targetCell.HasUnit && targetCell.Unit.TeamOwner != teamOwner)
                            {
                                // D. Phát hiện có Địch ngay đích đến -> Kích hoạt lệnh Đá Tiến (Action 3)
                                actionMask.SetActionEnabled(1, 3, true);
                            }
                            else if (!targetCell.HasUnit || targetCell.Unit.TeamOwner == teamOwner)
                            {
                                // E. Không có Địch -> Lệnh chỉ đơn giản Đi Bình Thường (Action 2)
                                actionMask.SetActionEnabled(1, 2, true);
                            }

                            unitCanDoSomething = true;
                        }

                        // Kiểm tra đường đi Lùi (Reverse / Đá Hậu)
                        var reversePath = pathfinding.GetReversePath(boardModel, currentCell, diceValue);
                        bool isInvalidReverse = currentCell.Structure == CellStructure.Spawn &&
                                                currentCell.TeamOwner == teamOwner;
                        if (!isInvalidReverse && reversePath.Count > 0 &&
                            !boardController.IsPathBlocked(reversePath, unit.TeamOwner))
                        {
                            var targetCell = reversePath.Last();
                            if (targetCell.HasUnit && targetCell.Unit.TeamOwner != teamOwner)
                            {
                                // F. Kỹ thuật cao: Đá Hậu - Phát hiện có Địch ở đường đi ngược. Mở khóa Action 4.
                                actionMask.SetActionEnabled(1, 4, true);
                                unitCanDoSomething = true;
                            }
                        }
                    }
                }

                // G. Đang làm trùm ở trong Chuồng và muốn bò lên bậc thang cao cấp hơn - Action 7
                if (currentCell != null && unit.State == UnitState.InHome)
                {
                    if (boardModel.HomeCells.TryGetValue(unit.TeamOwner, out var homeCells))
                    {
                        int targetIndex = diceValue - 1;
                        if (targetIndex > currentCell.Index && targetIndex < homeCells.Count)
                        {
                            bool isBlocked = false;
                            for (int i = currentCell.Index + 1; i <= targetIndex; i++)
                            {
                                if (homeCells[i].HasUnit) isBlocked = true;
                            }

                            // Nếu đường thông thoáng
                            if (!isBlocked)
                            {
                                actionMask.SetActionEnabled(1, 7, true);
                                unitCanDoSomething = true;
                            }
                        }
                    }
                }

                // 4. CHỐT HẠ ĐÓNG BĂNG QUÂN CỜ
                // Nếu Quân Cờ này hoàn toàn bế tắc (VD bị mắc kẹt ko nhúc nhích được do xúc xắc xấu)
                // Và AI cũng không có vật phẩm buff nào trong người dể sài.
                // Trực tiếp Khóa (Mask) nút bấm chọn Quân Cờ này trên Branch 0 (Branch chọn quân là u+1 vì 0 là nút Bỏ Qua Lượt)
                if (!unitCanDoSomething && !hasItems)
                {
                    actionMask.SetActionEnabled(0, u + 1, false);
                }
            }
        }
    }
}