using MADP.Models.Menu.Tutorial;
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

            Debug.Log($"ĐÃ XÁC NHẬN! Chuẩn bị Load Tutorial Map: {_selectedStep.TutorialMap}");
            
            //Bắt đầu gọi hàm LoadScene hoặc báo cho GameManager tại đây
        }
    }
}