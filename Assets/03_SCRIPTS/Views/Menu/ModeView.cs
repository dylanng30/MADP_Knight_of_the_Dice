using System;
using MADP.Controllers;
using MADP.Models.Menu.Tutorial;
using MADP.Views.Menu.BotMode;
using MADP.Views.Menu.TutorialMode;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views.Menu
{
    [Serializable]
    public struct ModeButton
    {
        public Button Button;
        public GameObject ChosenSign;
    }
    public class ModeView : MonoBehaviour
    {
        [Header("BUTTON SETTINGS")]
        [SerializeField] private ModeButton tutorialModeButton;
        [SerializeField] private ModeButton botModeButton;
        
        [Header("VIEWS")]
        [Space(10)]
        [SerializeField] private TutorialModeView tutorialModeView;
        [SerializeField] private BotModeView botModeView;
        
        public Action OnBotModeTabClicked;
        public Action OnTutorialModeTabClicked;
        
        private void Awake()
        {
            tutorialModeButton.Button.onClick.AddListener((() =>
            {
                SwitchMode(tutorialModeView.gameObject, tutorialModeButton.ChosenSign);
                OnTutorialModeTabClicked?.Invoke();
            }));
            
            botModeButton.Button.onClick.AddListener((() =>
            {
                SwitchMode(botModeView.gameObject, botModeButton.ChosenSign);
                OnBotModeTabClicked?.Invoke();
            }));
        }

        private void SwitchMode(GameObject activePanel, GameObject activeSign)
        {
            tutorialModeView.gameObject.SetActive(false);
            botModeView.gameObject.SetActive(false);
            
            tutorialModeButton.ChosenSign.SetActive(false);
            botModeButton.ChosenSign.SetActive(false);
            
            activePanel.SetActive(true);
            if (activeSign != null) activeSign.SetActive(true);
        }
        
        public void Show()
        {
            gameObject.SetActive(true);
            SwitchMode(tutorialModeView.gameObject,  tutorialModeButton.ChosenSign);
            OnTutorialModeTabClicked?.Invoke();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}