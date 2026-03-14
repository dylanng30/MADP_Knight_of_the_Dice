using System.Collections.Generic;
using MADP.Controllers;
using MADP.Services.Gold.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MADP.Tutorial
{
    // Điều phối viên hệ thống hướng dẫn (Tutorial System).
    // Chịu trách nhiệm khởi tạo các màn (stages), xử lý chuyển màn và lưu tiến trình.
    public class TutorialDirector : MonoBehaviour
    {
        [SerializeField] private TurnController turnController;
        [SerializeField] private BoardController boardController;
        [SerializeField] private TutorialUIOverlay tutorialUI;
        [SerializeField] private TutorialMapInitializer mapInitializer;

        private List<TutorialStageBase> _stages;
        private int _currentStageIndex;

        // Bắt đầu chuỗi hướng dẫn từ một màn cụ thể.
        public void StartTutorial(int startStageIndex, IGoldService goldService)
        {
            _currentStageIndex = startStageIndex;

            // Inject dependencies vào MapInitializer
            if (mapInitializer != null)
                mapInitializer.Initialize(boardController, goldService);

            // Khởi tạo danh sách các màn hướng dẫn
            _stages = new List<TutorialStageBase>
            {
                new TutorialStage1_Awakening(),
                new TutorialStage2_Summoning(),
                new TutorialStage3_Combat(),
                new TutorialStage4_SpecialCells(),
                new TutorialStage5_Shop()
            };

            foreach (var stage in _stages)
                stage.Initialize(turnController, boardController, tutorialUI, mapInitializer);

            turnController.OnDiceRolled += HandleDiceRolled;
            turnController.OnTurnActionCompletedEvent += HandleTurnCompleted;

            RunCurrentStage();
        }

        private void HandleDiceRolled(int value)
        {
            if (_stages != null && _currentStageIndex < _stages.Count)
                _stages[_currentStageIndex].OnDiceRolled(value);
        }

        private void HandleTurnCompleted()
        {
            if (_stages != null && _currentStageIndex < _stages.Count)
                _stages[_currentStageIndex].OnTurnCompleted();
        }

        private void OnDestroy()
        {
            if (turnController != null)
            {
                turnController.OnDiceRolled -= HandleDiceRolled;
                turnController.OnTurnActionCompletedEvent -= HandleTurnCompleted;
            }
        }

        // Thực thi màn hướng dẫn hiện tại.
        private void RunCurrentStage()
        {
            if (_currentStageIndex >= _stages.Count)
            {
                Debug.Log("[TutorialDirector] Tutorial hoàn thành!");
                return;
            }

            // Setup map đặc thù cho stage, sau đó bắt đầu stage
            mapInitializer.SetupForStage(_currentStageIndex, () =>
            {
                _stages[_currentStageIndex].Enter(GoToNextStage);
            });
        }

        // Chuyển sang màn hướng dẫn tiếp theo và lưu tiến trình.
        public void GoToNextStage()
        {
            _stages[_currentStageIndex].Exit();
            
            // Lưu tiến trình: Mở khóa stage tiếp theo
            int nextStage = _currentStageIndex + 1;
            int currentMax = PlayerPrefs.GetInt("MaxTutorialStage", 0);
            if (nextStage > currentMax)
            {
                PlayerPrefs.SetInt("MaxTutorialStage", nextStage);
                PlayerPrefs.Save();
            }

            // Hiện thông báo hoàn thành thay vì quay về ngay
            if (tutorialUI != null)
            {
                tutorialUI.ShowDialogue("Màn hướng dẫn đã hoàn thành! Nhấn để quay về menu.", () => 
                {
                    if (SceneController.Instance != null)
                    {
                        SceneController.Instance.LoadScene("Menu");
                    }
                    else
                    {
                        SceneManager.LoadScene("Menu");
                    }
                });
            }
            else
            {
                if (SceneController.Instance != null)
                {
                    SceneController.Instance.LoadScene("Menu");
                }
                else
                {
                    SceneManager.LoadScene("Menu");
                }
            }
        }
    }
}
