using System;
using System.Collections.Generic;
using MADP.Models.Menu.Tutorial;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views.Menu.TutorialMode
{
    public class TutorialModeView : MonoBehaviour
    {
        [SerializeField] private Transform stepContainer;
        [SerializeField] private TutorialStepItemView stepItemPrefab;
        [SerializeField] private Button confirmButton;
        
        public Action<TutorialStepModel> OnStepSelected;
        public Action OnConfirmClicked;
        
        private List<TutorialStepItemView> _spawnedItems = new ();
        
        private void Awake()
        {
            if (confirmButton != null)
                confirmButton.onClick.AddListener(() => OnConfirmClicked?.Invoke());
        }
        
        public void Initialize(TutorialRouteModel routeModel)
        {
            ClearContainer();

            if (routeModel == null || routeModel.Steps == null) return;

            foreach (var step in routeModel.Steps)
            {
                TutorialStepItemView stepView = Instantiate(stepItemPrefab, stepContainer);
                stepView.Setup(step, (selectedModel) => OnStepSelected?.Invoke(selectedModel));
                _spawnedItems.Add(stepView);
            }
        }
        
        public void UpdateSelection(TutorialStepModel selectedStep)
        {
            foreach (var item in _spawnedItems)
            {
                item.SetSelected(item.Model == selectedStep);
            }
        }
        
        private void ClearContainer()
        {
            foreach (var item in _spawnedItems)
            {
                Destroy(item.gameObject);
            }
            
            _spawnedItems.Clear();
        }
    }
}