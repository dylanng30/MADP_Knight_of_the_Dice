using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Tutorial
{
    public class TutorialUIOverlay : MonoBehaviour
    {
        [Header("Dialogue UI")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Button confirmButton;

        [Header("Pointer UI")]
        [SerializeField] private RectTransform pointerRect;

        [Header("Message UI")]
        [SerializeField] private GameObject messagePanel;
        [SerializeField] private TextMeshProUGUI messageText;

        [Header("Input Mask")]
        [Tooltip("Panel chặn click vào game world. Đặt ở SAU DialoguePanel trong Hierarchy để dialogue luôn render đè lên trên mask.")]
        [SerializeField] private GameObject inputMask;

        private Action _onDialogueConfirmed;

        private void Awake()
        {
            if (confirmButton != null)
                confirmButton.onClick.AddListener(HandleConfirmClicked);
            
            HideAll();
        }

        private void HideAll()
        {
            if (dialoguePanel) dialoguePanel.SetActive(false);
            if (pointerRect) pointerRect.gameObject.SetActive(false);
            if (messagePanel) messagePanel.SetActive(false);
            if (inputMask) inputMask.SetActive(false);
        }

        public void ShowDialogue(string text, Action onConfirmed)
        {
            _onDialogueConfirmed = onConfirmed;
            if (dialogueText) dialogueText.text = text;
            if (inputMask) inputMask.SetActive(true);
            if (dialoguePanel) dialoguePanel.SetActive(true);
        }

        public void HideDialogue()
        {
            if (dialoguePanel) dialoguePanel.SetActive(false);
        }

        private void HandleConfirmClicked()
        {
            HideDialogue();
            if (inputMask) inputMask.SetActive(false);
            _onDialogueConfirmed?.Invoke();
        }

        public void ShowPointerAtUI(RectTransform target)
        {
            if (pointerRect == null) return;
            pointerRect.gameObject.SetActive(true);
            pointerRect.position = target.position;
        }

        public void ShowPointerAtWorld(Vector3 worldPos)
        {
            if (pointerRect == null) return;
            pointerRect.gameObject.SetActive(true);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            pointerRect.position = screenPos;
        }

        public void HidePointer()
        {
            if (pointerRect) pointerRect.gameObject.SetActive(false);
        }

        public void ShowMessage(string text, float duration)
        {
            if (messageText) messageText.text = text;
            if (messagePanel)
            {
                messagePanel.SetActive(true);
                CancelInvoke(nameof(HideMessage));
                Invoke(nameof(HideMessage), duration);
            }
        }

        private void HideMessage()
        {
            if (messagePanel) messagePanel.SetActive(false);
        }

        public void SetInputMask(bool active)
        {
            if (inputMask) inputMask.SetActive(active);
        }
    }
}
