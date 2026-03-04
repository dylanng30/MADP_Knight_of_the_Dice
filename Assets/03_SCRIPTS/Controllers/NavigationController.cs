using System;
using MADP.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Controllers
{
    public class NavigationController : MonoBehaviour
    {
        [Header("---MODE---")] 
        [SerializeField] private ModeController modeController;
        [SerializeField] private Button playButton;
        
        
        [Header("---LOBBY---")]
        [SerializeField] private LobbyController lobbyController;
        
        [Space(10)]
        [SerializeField] private Button homeButton;
        
        private Action OnShowModeViewRequested;
        private Action OnBackToHomeRequested;

        private void Start()
        {
            Initilize();
            HideAll();
            ShowModeView();
        }

        private void Initilize()
        {
            if (homeButton)
            {
                OnBackToHomeRequested += HideModeView;
                homeButton.onClick.AddListener(OnBackToHomeRequested.Invoke);
            }

            if (modeController != null && playButton != null)
            {
                modeController.Initialize();
                modeController.OnStartBotModeRequested += HandleStartBotMode;
                OnShowModeViewRequested += HideAll;
                OnShowModeViewRequested += ShowModeView;
                playButton.onClick.AddListener(OnShowModeViewRequested.Invoke);
            }
        }
        
        private void HandleStartBotMode(BotDifficulty difficulty)
        {
            HideModeView();
            
            if (lobbyController != null)
            {
                lobbyController.CreateLobby(1);
                lobbyController.SetupBotDifficulty(difficulty);
                lobbyController.ShowLobbyView();
            }
        }

        private void ShowModeView()
        {
            playButton.interactable = false;
            modeController.ShowModeView();
        }

        private void HideModeView()
        {
            playButton.interactable = true;
            modeController.HideModeView();
        }

        private void HideAll()
        {
            if(modeController != null) modeController.HideModeView();
            if(lobbyController != null) lobbyController.HideLobbyView();
        }

        private void OnDisable()
        {
            OnShowModeViewRequested -= HideAll;
            OnShowModeViewRequested -= modeController.ShowModeView;
            modeController.OnStartBotModeRequested -= HandleStartBotMode;
        }
    }
}