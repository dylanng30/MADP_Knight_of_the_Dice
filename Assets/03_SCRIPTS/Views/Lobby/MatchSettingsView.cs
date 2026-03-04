using System;
using MADP.Models;
using MADP.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views.Lobby
{
    public class MatchSettingsView : MonoBehaviour
    {
        [SerializeField] private Button saveButton;
        [SerializeField] private Button exitButton;
        
        [Header("---Map---")]
        [SerializeField] private Image mapImage;
        [SerializeField] private TextMeshProUGUI mapNameText;
        [SerializeField] private Button nextMapButton;
        [SerializeField] private Button prevMapButton;
        
        [Header("---TIME PER TURN---")]
        [SerializeField] private TextMeshProUGUI timePerTurnText;
        [SerializeField] private Button increaseTimeButton;
        [SerializeField] private Button decreaseTimeButton;
        
        [Header("---HARM---")]
        [SerializeField] private TextMeshProUGUI redText;
        [SerializeField] private Button increaseRedButton;
        [SerializeField] private Button decreaseRedButton;
        
        [Header("---GOLD---")]
        [SerializeField] private TextMeshProUGUI yellowText;
        [SerializeField] private Button increaseYellowButton;
        [SerializeField] private Button decreaseYellowButton;
        
        [Header("---MYTH---")]
        [SerializeField] private TextMeshProUGUI purpleText;
        [SerializeField] private Button increasePurpleButton;
        [SerializeField] private Button decreasePurpleButton;
        
        [Header("---HEAL---")]
        [SerializeField] private TextMeshProUGUI greenText;
        [SerializeField] private Button increaseGreenButton;
        [SerializeField] private Button decreaseGreenButton;
        
        
        private MapType _currentSelectedMap;
        private int _currentSelectedTime;
        private int _currentRedCells;
        private int _currentYellowCells;
        private int _currentPurpleCells;
        private int _currentGreenCells;
        
        public Action<int, MapType, int, int, int, int> OnSettingsSaved;
        
        private const int MIN_TIME = 10;
        private const int MAX_TIME = 60;
        private const int MIN_SPECIAL = 0;
        private const int MAX_SPECIAL = 3;
        
        private void Awake()
        {
            saveButton.onClick.AddListener(SaveAndHide);
            exitButton.onClick.AddListener(Hide);
            
            prevMapButton.onClick.AddListener(() => ChangeMap(-1));
            nextMapButton.onClick.AddListener(() => ChangeMap(1));

            decreaseTimeButton.onClick.AddListener(() => ChangeTime(-5));
            increaseTimeButton.onClick.AddListener(() => ChangeTime(5));

            decreaseRedButton.onClick.AddListener(() => ChangeRed(-1));
            increaseRedButton.onClick.AddListener(() => ChangeRed(1));

            decreaseYellowButton.onClick.AddListener(() => ChangeYellow(-1));
            increaseYellowButton.onClick.AddListener(() => ChangeYellow(1));

            decreasePurpleButton.onClick.AddListener(() => ChangePurple(-1));
            increasePurpleButton.onClick.AddListener(() => ChangePurple(1));
            
            decreaseGreenButton.onClick.AddListener(() => ChangeGreen(-1));
            increaseGreenButton.onClick.AddListener(() => ChangeGreen(1));
        }

        public void Show(MatchSettingsModel currentSettings)
        {
            _currentSelectedMap = currentSettings.SelectedMap;
            _currentSelectedTime = currentSettings.TimePerTurn;
            _currentRedCells = currentSettings.RedCellCount;
            _currentYellowCells = currentSettings.YellowCellCount;
            _currentPurpleCells = currentSettings.PurpleCellCount;
            _currentGreenCells = currentSettings.GreenCellCount;
            
            RefreshAllUI();
            gameObject.SetActive(true);
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        private void RefreshAllUI()
        {
            mapNameText.text = _currentSelectedMap.ToString();
            timePerTurnText.text = $"{_currentSelectedTime}s";
            redText.text = _currentRedCells.ToString();
            yellowText.text = _currentYellowCells.ToString();
            purpleText.text = _currentPurpleCells.ToString();
            greenText.text = _currentGreenCells.ToString();
        }
        
        private void ChangeMap(int direction)
        {
            int mapCount = Enum.GetValues(typeof(MapType)).Length;
            int newIndex = ((int)_currentSelectedMap + direction + mapCount) % mapCount;
            
            _currentSelectedMap = (MapType)newIndex;
            mapNameText.text = _currentSelectedMap.ToString();
        }

        private void ChangeTime(int delta)
        {
            _currentSelectedTime = Mathf.Clamp(_currentSelectedTime + delta, MIN_TIME, MAX_TIME);
            timePerTurnText.text = $"{_currentSelectedTime}s";
        }

        private void ChangeRed(int delta)
        {
            _currentRedCells = Mathf.Clamp(_currentRedCells + delta, MIN_SPECIAL, MAX_SPECIAL);
            redText.text = _currentRedCells.ToString();
        }

        private void ChangeYellow(int delta)
        {
            _currentYellowCells = Mathf.Clamp(_currentYellowCells + delta, MIN_SPECIAL, MAX_SPECIAL);
            yellowText.text = _currentYellowCells.ToString();
        }

        private void ChangePurple(int delta)
        {
            _currentPurpleCells = Mathf.Clamp(_currentPurpleCells + delta, MIN_SPECIAL, MAX_SPECIAL);
            purpleText.text = _currentPurpleCells.ToString();
        }
        private void ChangeGreen(int delta)
        {
            _currentGreenCells = Mathf.Clamp(_currentGreenCells + delta, MIN_SPECIAL, MAX_SPECIAL);
            greenText.text = _currentGreenCells.ToString();
        }
        
        private void SaveAndHide()
        {
            OnSettingsSaved?.Invoke(
                _currentSelectedTime,
                _currentSelectedMap,
                _currentRedCells,
                _currentYellowCells,
                _currentPurpleCells,
                _currentGreenCells);
            Hide();
        }
    }
}