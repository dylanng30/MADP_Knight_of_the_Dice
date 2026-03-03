using System;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views
{
    public class RollButtonView : MonoBehaviour
    {
        [SerializeField] private Button rollButton;
        
        public Action OnRollClicked;
        
        private void Start()
        {
            if (rollButton != null)
            {
                rollButton.onClick.AddListener(() => OnRollClicked?.Invoke());
            }
        }

        public void SetInteractable(bool isInteractable)
        {
            if (rollButton != null)
            {
                rollButton.interactable = isInteractable;
            }
        }
        
        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
    }
}