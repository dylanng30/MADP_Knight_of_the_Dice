using System;
using MADP.Settings;
using MADP.Views.Menu.BotMode;
using UnityEngine;

namespace MADP.Controllers
{
    public class BotModeController : MonoBehaviour
    {
        [SerializeField] private BotModeView botModeView;
            
        private BotDifficulty _savedDifficulty;
        
        public Action<BotDifficulty> OnConfirmPlay;
        
        public void Initialize()
        {
            botModeView.OnDifficultySelected += HandleDifficultySelected;
            botModeView.OnConfirmClicked += HandleConfirmClicked;
        }
        
        public void OnOpen()
        {
            _savedDifficulty = BotDifficulty.Easy;
            botModeView.Setup(_savedDifficulty);
        }

        private void HandleDifficultySelected(BotDifficulty difficulty)
        {
            _savedDifficulty = difficulty;
            botModeView.Setup(_savedDifficulty);
        }

        private void HandleConfirmClicked()
        {
            Debug.Log($"ĐÃ XÁC NHẬN! Chuẩn bị vào game với độ khó: {_savedDifficulty}");
            
            OnConfirmPlay?.Invoke(_savedDifficulty);
        }
    }
}