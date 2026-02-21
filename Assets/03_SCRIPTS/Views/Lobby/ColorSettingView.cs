using System;
using System.Collections.Generic;
using MADP.Models;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views.Lobby
{
    public class ColorSettingView : MonoBehaviour
    {
        [SerializeField] private List<ColorItemView> colorItemViews;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button exitButton;
        
        private int _currentSlotIndex;
        private TeamColor _currentColor;
        private List<TeamColor> _takenColors;
        
        public Action<int, TeamColor> OnColorSaved;

        private void Awake()
        {
            saveButton.onClick.AddListener(() => OnColorSaved?.Invoke(_currentSlotIndex, _currentColor));
            exitButton.onClick.AddListener(Hide);

            foreach (var item in colorItemViews)
                item.Initialize(OnColorItemClicked);
        }

        public void Show(TeamColor currentColor, List<TeamColor> takenColors, int slotIndex)
        {
            _currentSlotIndex = slotIndex;
            _currentColor = currentColor;
            _takenColors = takenColors;
            
            RefreshColorItems();
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);

        private void OnColorItemClicked(TeamColor newColor)
        {
            _currentColor = newColor;
            RefreshColorItems(); 
        }

        private void RefreshColorItems()
        {
            foreach (var item in colorItemViews)
            {
                bool isTaken = _takenColors.Contains(item.TeamColor);
                bool isSelected = (item.TeamColor == _currentColor);
                item.Setup(isTaken, isSelected);
            }
        }
    }
}