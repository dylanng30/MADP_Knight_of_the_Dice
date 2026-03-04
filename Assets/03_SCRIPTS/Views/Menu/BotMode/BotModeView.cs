using System;
using MADP.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views.Menu.BotMode
{
    public class BotModeView : MonoBehaviour
    {
        [Header("---TOGGLES---")]
        [SerializeField] private Toggle easyToggle;
        [SerializeField] private Toggle mediumToggle;
        [SerializeField] private Toggle hardToggle;
        [SerializeField] private Toggle superHardToggle;
        
        [Header("---BUTTONS---")]
        [SerializeField] private Button confirmButton;
        
        public Action<BotDifficulty> OnDifficultySelected;
        public Action OnConfirmClicked;
        
        private void Awake()
        {
            easyToggle.onValueChanged.AddListener(isOn => { if (isOn) OnDifficultySelected?.Invoke(BotDifficulty.Easy); });
            mediumToggle.onValueChanged.AddListener(isOn => { if (isOn) OnDifficultySelected?.Invoke(BotDifficulty.Medium); });
            hardToggle.onValueChanged.AddListener(isOn => { if (isOn) OnDifficultySelected?.Invoke(BotDifficulty.Hard); });
            superHardToggle.onValueChanged.AddListener(isOn => { if (isOn) OnDifficultySelected?.Invoke(BotDifficulty.SuperHard); });
            
            confirmButton.onClick.AddListener(() => OnConfirmClicked?.Invoke());
        }
        
        public void Setup(BotDifficulty currentDifficulty)
        {
            easyToggle.SetIsOnWithoutNotify(currentDifficulty == BotDifficulty.Easy);
            mediumToggle.SetIsOnWithoutNotify(currentDifficulty == BotDifficulty.Medium);
            hardToggle.SetIsOnWithoutNotify(currentDifficulty == BotDifficulty.Hard);
            superHardToggle.SetIsOnWithoutNotify(currentDifficulty == BotDifficulty.SuperHard);
        }
    }
}