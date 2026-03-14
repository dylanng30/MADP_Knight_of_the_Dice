using System;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views
{
    public class EndTurnButtonView : MonoBehaviour
    {
        [SerializeField] private Button endTurnButton;

        public Action OnEndClicked;
        public RectTransform Rect => transform as RectTransform;

        private void Start()
        {
            if (endTurnButton != null)
            {
                endTurnButton.onClick.AddListener(() => OnEndClicked?.Invoke());
            }
        }
        
        public void SetInteractable(bool isInteractable)
        {
            if (endTurnButton != null)
            {
                endTurnButton.interactable = isInteractable;
            }
        }
        
        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
    }
}