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
        public GameObject ChosenSign;
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
            generalButton.Button.onClick.AddListener(() =>
            {
                SwitchTab(generalSettingsView.gameObject, generalButton.ChosenSign);
            });
            
            soundButton.Button.onClick.AddListener(() =>
            {
                SwitchTab(soundSettingsView.gameObject, soundButton.ChosenSign);
            });
            
            //MAIN BUTTONS
            saveButton.onClick.AddListener(() => OnSaveClicked?.Invoke());
            exitButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
        }

        public void Initialize(GameSettingsModel currentSettings)
        {
            soundSettingsView.Setup(currentSettings);
            
            generalButton.ChosenSign.gameObject.SetActive(false);
            soundButton.ChosenSign.gameObject.SetActive(false);
            
            gameObject.SetActive(false);
        }
        
        private void SwitchTab(GameObject activePanel, GameObject activeSign)
        {
            generalSettingsView.gameObject.SetActive(false);
            soundSettingsView.gameObject.SetActive(false);
            
            generalButton.ChosenSign.SetActive(false);
            soundButton.ChosenSign.SetActive(false);
            
            activePanel.SetActive(true);
            if (activeSign != null) 
            {
                activeSign.SetActive(true);
            }
        }

        public void Show()
        {
            Time.timeScale = 0;
            gameObject.SetActive(true);
            SwitchTab(generalSettingsView.gameObject,  generalButton.ChosenSign);
        }

        public void Hide()
        {
            Time.timeScale = 1;
            gameObject.SetActive(false);
        } 
    }
}