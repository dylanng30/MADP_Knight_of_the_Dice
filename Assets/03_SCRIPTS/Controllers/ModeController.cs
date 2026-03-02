using System;
using MADP.Settings;
using MADP.Views.Menu;
using MADP.Views.Menu.BotMode;
using UnityEngine;

namespace MADP.Controllers
{
    public class ModeController : MonoBehaviour
    {
        [Header("---VIEWS---")]
        [SerializeField] private ModeView modeView;
        
        [Space(10)]
        [Header("---CONTROLLERS---")]
        [SerializeField] private TutorialModeController tutorialModeController;
        [SerializeField] private BotModeController botModeController;
        
        public Action<BotDifficulty> OnStartBotModeRequested;
        
        public void Initialize()
        {
            tutorialModeController.Initialize();
            botModeController.Initialize();
            
            modeView.OnBotModeTabClicked += botModeController.OnOpen;
            modeView.OnTutorialModeTabClicked += tutorialModeController.OnOpen;
            
            botModeController.OnConfirmPlay += (difficulty) => OnStartBotModeRequested?.Invoke(difficulty);
        }

        public void ShowModeView()
        {
            modeView.Show();
        }

        public void HideModeView()
        {
            modeView.Hide();
        }
    }
}