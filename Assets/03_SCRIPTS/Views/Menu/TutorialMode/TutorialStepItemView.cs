using System;
using MADP.Models.Menu.Tutorial;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MADP.Views.Menu.TutorialMode
{
    public class TutorialStepItemView : MonoBehaviour
    {
        [SerializeField] private Image mapAvatarImage;
        [SerializeField] private GameObject lockOverlay;
        
        [Header("--- Selection ---")]
        [SerializeField] private GameObject highlightObj; 
        [SerializeField] private Button selectButton;

        public TutorialStepModel Model { get; private set; }
        
        public void Setup(TutorialStepModel model, Action<TutorialStepModel> onSelected)
        {
            Model = model;
            
            if (model.MapAvatar != null) mapAvatarImage.sprite = model.MapAvatar;

            lockOverlay.SetActive(!model.IsOpened); 
            
            selectButton.interactable = model.IsOpened; 
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => onSelected?.Invoke(Model));
        }
        
        public void SetSelected(bool isSelected)
        {
            if (highlightObj != null)
                highlightObj.SetActive(isSelected);
        }
    }
}