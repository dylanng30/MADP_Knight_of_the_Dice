using MADP.Managers;
using MADP.Models;
using MADP.Models.Menu.Tutorial;
using MADP.Settings;
using MADP.Views.Menu.TutorialMode;
using UnityEngine;

namespace MADP.Controllers
{
    public class TutorialModeController : MonoBehaviour
    {
        [SerializeField] private TutorialRouteModel routeModel;
        [SerializeField] private TutorialModeView tutorialModeView;
        
        private TutorialStepModel _selectedStep;

        public void Initialize()
        {
            tutorialModeView.Initialize(routeModel);
            
            tutorialModeView.OnStepSelected += HandleStepSelected;
            tutorialModeView.OnConfirmClicked += HandleConfirmClicked;
        }
        
        public void OnOpen()
        {
            // Reset tiến trình về màn 1 theo yêu cầu
            // PlayerPrefs.SetInt("MaxTutorialStage", 0);
            // PlayerPrefs.Save();

            int maxOpened = PlayerPrefs.GetInt("MaxTutorialStage", 0);
            
            for (int i = 0; i < routeModel.Steps.Count; i++)
            {
                // Stage 0 luôn mở, các stage khác mở nếu index <= maxOpened
                routeModel.Steps[i].IsOpened = (i <= maxOpened);
            }

            Debug.Log($"[Tutorial] MaxStage detected: {maxOpened}");
            tutorialModeView.Initialize(routeModel);

            _selectedStep = GetHighestOpenedStep();
            
            if (_selectedStep != null)
            {
                tutorialModeView.UpdateSelection(_selectedStep);
            }
        }

        private TutorialStepModel GetHighestOpenedStep()
        {
            if (routeModel == null || routeModel.Steps == null || routeModel.Steps.Count == 0)
                return null;
            
            for (int i = routeModel.Steps.Count - 1; i >= 0; i--)
            {
                if (routeModel.Steps[i].IsOpened)
                    return routeModel.Steps[i];
            }
            
            return routeModel.Steps[0]; 
        }

        private void HandleStepSelected(TutorialStepModel step)
        {
            if (!step.IsOpened) return;

            _selectedStep = step;
            
            tutorialModeView.UpdateSelection(_selectedStep);
            
            Debug.Log($"Đang chọn Map: {step.TutorialMap}");
        }

        private void HandleConfirmClicked()
        {
            if (_selectedStep == null) return;

            int stageIndex = routeModel.Steps.IndexOf(_selectedStep);

            var settings = new MatchSettingsModel
            {
                IsTutorial = true,
                TutorialStageIndex = stageIndex,
                SelectedMap = MapType.Desert,
                TimePerTurn = 9999,
                RedCellCount = 0,
                YellowCellCount = 0,
                PurpleCellCount = 0,
                GreenCellCount = 0,
                Slots = new[]
                {
                    new LobbySlotModel(0, TeamColor.Red) { PlayerType = PlayerType.Human }
                }
            };
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CurrentMatchSettings = settings;
                if (SceneController.Instance != null)
                {
                    SceneController.Instance.LoadScene("TutorialDesert");
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("TutorialDesert");
                }
            }
        }
    }
}