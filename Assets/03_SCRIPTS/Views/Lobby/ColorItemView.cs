using System;
using MADP.Models;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views.Lobby
{
    public class ColorItemView : MonoBehaviour
    {
        [SerializeField] private Button chooseColorButton;
        [SerializeField] private Image itemView;
        [SerializeField] private GameObject chooseFrame;
        [SerializeField] private TeamColor teamColor;
        public TeamColor TeamColor => teamColor;
        private Action<TeamColor> _onSelect;

        public void Initialize(Action<TeamColor> onSelect)
        {
            _onSelect = onSelect;
            chooseColorButton.onClick.AddListener(() => _onSelect?.Invoke(teamColor));
        }

        public void Setup(bool isTaken, bool isSelected)
        {
            chooseFrame.SetActive(isSelected);
            chooseColorButton.interactable = !isTaken && !isSelected;
        }
    }
}