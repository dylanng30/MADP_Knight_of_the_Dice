using System;
using System.Collections.Generic;
using MADP.Models;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views.Lobby
{
    public class RoleSettingView : MonoBehaviour
    {
        [SerializeField] private List<RoleItemView> roleItemViews;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button saveButton;

        private int _currentSlotIndex;
        private RoleType _currentSelectedRole;
        
        public Action<int, RoleType> OnRoleSaved;

        private void Awake()
        {
            saveButton.onClick.AddListener(() => OnRoleSaved?.Invoke(_currentSlotIndex, _currentSelectedRole));
            exitButton.onClick.AddListener(Hide);
            
            foreach (var item in roleItemViews)
            {
                item.Initialize(OnRoleItemClicked);
            }
        }
        public void Show(RoleType currentRoleType, int slotIndex)
        {
            _currentSlotIndex = slotIndex;
            _currentSelectedRole = currentRoleType;
            RefreshItems();
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);

        private void OnRoleItemClicked(RoleType newRole)
        {
            _currentSelectedRole = newRole;
            RefreshItems();
        }

        private void RefreshItems()
        {
            foreach (var item in roleItemViews)
            {
                item.Setup(item.RoleType == _currentSelectedRole);
            }
        }
    }
}