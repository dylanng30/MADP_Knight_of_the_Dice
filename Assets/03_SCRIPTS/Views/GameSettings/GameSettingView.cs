using System;
using MADP.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views.GameSettings
{
    [Serializable]
    public struct GameSettingsButton
    {
        public Button Button;
        public GameObject ChooseSign;
    }
    public class GameSettingView : MonoBehaviour
    {
        [Header("---MAIN BUTTONS---")]
        [SerializeField] private Button exitButton;
        [SerializeField] private Button saveButton;
        
        [Header("---SIDE BAR---")]
        [SerializeField] private GameSettingsButton generalButton;
        [SerializeField] private GameSettingsButton soundButton;
        
        [Header("---SUB SETTINGS---")]
        [SerializeField] private GeneralSettingsView generalSettingsView;
        [SerializeField] private SoundSettingsView soundSettingsView;
        
        public GeneralSettingsView GeneralPanel => generalSettingsView;
        public SoundSettingsView SoundPanel => soundSettingsView;

        
        
        
        public Action OnSaveClicked;
        public Action OnCloseClicked;

        private void Awake()
        {
            //SIDE BAR
            generalButton.Button.onClick.AddListener(() => SwitchTab(generalSettingsView.gameObject));
            soundButton.Button.onClick.AddListener(() => SwitchTab(soundSettingsView.gameObject));
            
            //MAIN BUTTONS
            saveButton.onClick.AddListener(() => OnSaveClicked?.Invoke());
            exitButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
        }

        public void Initialize(GameSettingsModel currentSettings)
        {
            soundSettingsView.Setup(currentSettings);
            
            generalButton.ChooseSign.gameObject.SetActive(false);
            soundButton.ChooseSign.gameObject.SetActive(false);
            
            gameObject.SetActive(false);
        }
        
        private void SwitchTab(GameObject activePanel)
        {
            generalSettingsView.gameObject.SetActive(false);
            soundSettingsView.gameObject.SetActive(false);
            
            //Chưa hiển thị đúng sign của button
            
            activePanel.SetActive(true);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            //generalButton.ChooseSign.gameObject.SetActive(true);
            SwitchTab(generalSettingsView.gameObject);
        } 
        public void Hide() => gameObject.SetActive(false);
    }
}