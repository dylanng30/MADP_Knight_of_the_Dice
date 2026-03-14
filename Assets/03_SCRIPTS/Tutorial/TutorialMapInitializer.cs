using System;
using System.Collections;
using System.Collections.Generic;
using MADP.Controllers;
using MADP.Models;
using MADP.Services.Gold.Interfaces;
using UnityEngine;

namespace MADP.Tutorial
{
    // Chịu trách nhiệm thiết lập trạng thái bàn cờ ban đầu cho từng màn hướng dẫn.
    // Bao gồm: xóa bàn cờ, đặt quân, set vàng, và tạo các ô thuộc tính đặc biệt.
    public class TutorialMapInitializer : MonoBehaviour
    {
        [SerializeField] private BoardController boardController;
        private IGoldService _goldService;

        public void Initialize(BoardController board, IGoldService goldService)
        {
            boardController = board;
            _goldService = goldService;
        }

        // Thiết lập bàn cờ cho một Stage cụ thể dựa trên chỉ số index.
        public void SetupForStage(int stageIndex, Action onSetupComplete)
        {
            StartCoroutine(SetupCoroutine(stageIndex, onSetupComplete));
        }

        private IEnumerator SetupCoroutine(int stageIndex, Action onSetupComplete)
        {
            yield return null;

            // Dọn dẹp map trước khi setup stage mới (Xóa hết các ô đặc biệt cũ)
            if (boardController != null && boardController.Board != null)
            {
                foreach (var cell in boardController.Board.AroundCells)
                    cell.SetAttribute(CellAttribute.None);
                
                foreach (var kvp in boardController.Board.HomeCells)
                {
                    foreach (var cell in kvp.Value)
                        cell.SetAttribute(CellAttribute.None);
                }
            }

            // Gọi hàm setup tương ứng với từng stage
            switch (stageIndex)
            {
                case 0: SetupStage1(onSetupComplete); break;
                case 1: SetupStage2(onSetupComplete); break;
                case 2: SetupStage3(onSetupComplete); break;
                case 3: SetupStage4(onSetupComplete); break;
                case 4: SetupStage5(onSetupComplete); break;
                default: onSetupComplete?.Invoke(); break;
            }
        }

        private void SetupStage1(Action onComplete)
        {
            SetGold(TeamColor.Red, 20);

            // Spawn quân ta tại ô Spawn xuất phát (Thường là ô 0, 14, 28, 42 tùy map)
            // Lấy ô Spawn đầu tiên của Red
            var spawnCell = boardController.GetSpawnCell(TeamColor.Red);
            int spawnIndex = spawnCell != null ? spawnCell.Index : 0;

            int boardSize = boardController.Board.AroundCells.Count;
            SpawnUnitAt(TeamColor.Red, spawnIndex, () =>
            {
                // Spawn quân địch vật cản tại Spawn + 4
                SpawnUnitAt(TeamColor.Blue, (spawnIndex + 4) % boardSize, () =>
                {
                    onComplete?.Invoke();
                }, 1); // Lấy unit thứ 2 trong deck làm vật cản
            });
        }

        private void SetupStage2(Action onComplete)
        {
            SetGold(TeamColor.Red, 12);
            onComplete?.Invoke();
        }

        private void SetupStage3(Action onComplete)
        {
            SetGold(TeamColor.Red, 30);
            
            var spawnCell = boardController.GetSpawnCell(TeamColor.Red);
            int baseIdx = spawnCell != null ? spawnCell.Index : 0;
            int boardSize = boardController.Board.AroundCells.Count;

            // Đặt quân ta ở ô baseIdx + 5 (xa ô Spawn)
            SpawnUnitAt(TeamColor.Red, (baseIdx + 5) % boardSize, () =>
            {
                // Địch-1 (Trước) @ ô A+9 (cách 4 bước). HP đầy.
                SpawnUnitAt(TeamColor.Blue, (baseIdx + 9) % boardSize, () =>
                {
                    // Địch-2 (Sau) @ ô A+2 (cách lùi 3 bước). HP thấp.
                    SpawnUnitAt(TeamColor.Blue, (baseIdx + 2) % boardSize, () =>
                    {
                        var blueUnits = boardController.GetAllUnitsByColor(TeamColor.Blue);
                        if (blueUnits.Count > 1)
                        {
                            var target = blueUnits[1]; // Địch-2 phía sau
                            target.SetHealth(1); 
                        }

                        onComplete?.Invoke();
                    }, 1);
                }, 0);
            });
        }

        private void SetupStage4(Action onComplete)
        {
            SetGold(TeamColor.Red, 20);
            
            var spawnCell = boardController.GetSpawnCell(TeamColor.Red);
            int baseIdx = spawnCell != null ? spawnCell.Index : 0;
            int boardSize = boardController.Board.AroundCells.Count;

            // Đặt quân ta ở ô baseIdx + 4. Giảm máu để test ô Heal.
            SpawnUnitAt(TeamColor.Red, (baseIdx + 4) % boardSize, () =>
            {
                var units = boardController.GetAllUnitsByColor(TeamColor.Red);
                foreach (var u in units)
                {
                    if (u.State == UnitState.Moving)
                    {
                        u.TakeDamage(u.MaxHealth / 2); // Giảm 50% máu
                        break;
                    }
                }

                // Thiết lập 4 ô đặc biệt kế tiếp: baseIdx + 6, 8, 10, 12 (Cách 2 bước)
                boardController.Board.AroundCells[(baseIdx + 6) % boardSize].SetAttribute(CellAttribute.Heal);
                boardController.Board.AroundCells[(baseIdx + 8) % boardSize].SetAttribute(CellAttribute.Gold);
                boardController.Board.AroundCells[(baseIdx + 10) % boardSize].SetAttribute(CellAttribute.Myth);
                boardController.Board.AroundCells[(baseIdx + 12) % boardSize].SetAttribute(CellAttribute.Harm);

                onComplete?.Invoke();
            });
        }

        private void SetupStage5(Action onComplete)
        {
            SetGold(TeamColor.Red, 200); // Rất nhiều vàng để mua đồ
            
            var spawnCell = boardController.GetSpawnCell(TeamColor.Red);
            int baseIdx = spawnCell != null ? spawnCell.Index : 0;
            int boardSize = boardController.Board.AroundCells.Count;

            // Đặt quân ta ở ô baseIdx + 10
            SpawnUnitAt(TeamColor.Red, (baseIdx + 10) % boardSize, () =>
            {
                onComplete?.Invoke();
            });
        }

        private void SetGold(TeamColor team, int amount)
        {
            if (_goldService == null) return;
            int current = _goldService.GetGold(team);
            int diff = amount - current;
            if (diff > 0) _goldService.AddGold(team, diff);
        }

        public void SpawnUnitAt(TeamColor team, int cellIndex, Action onSpawned, int unitIndex = 0)
        {
            if (boardController == null)
            {
                onSpawned?.Invoke();
                return;
            }

            var allUnits = boardController.GetAllUnitsByColor(team);
            if (allUnits == null || allUnits.Count <= unitIndex)
            {
                onSpawned?.Invoke();
                return;
            }

            UnitModel unitToSpawn = allUnits[unitIndex];
            boardController.ForceSpawnAtCell(unitToSpawn, cellIndex, onSpawned);
        }
    }
}
