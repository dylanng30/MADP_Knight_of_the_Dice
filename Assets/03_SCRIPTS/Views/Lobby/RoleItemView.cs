using System;
using MADP.Models;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views.Lobby
{
    public class RoleItemView : MonoBehaviour
    {
        [SerializeField] private RoleType roleType;
        [SerializeField] private GameObject ticker;
        [SerializeField] private Button chooseRoleButton;
        
        public RoleType RoleType => roleType;
        private Action<RoleType> _onSelect;

        public void Initialize(Action<RoleType> onSelect)
        {
            _onSelect = onSelect;
            chooseRoleButton.onClick.AddListener(() => _onSelect?.Invoke(roleType));
        }

        public void Setup(bool isSelected)
        {
            ticker.SetActive(isSelected);
            chooseRoleButton.interactable = !isSelected;
        }
    }
}