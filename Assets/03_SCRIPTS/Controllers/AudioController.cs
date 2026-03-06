using MADP.Managers;
using MADP.Models;
using MADP.Models.UnitActions;
using MADP.Services.Audio;
using MADP.Services.Audio.Interfaces;
using MADP.Services.Gold.Interfaces;
using MADP.Settings;
using MADP.Systems;
using MADP.Utilities;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace MADP.Controllers
{
    public class AudioController : PersistentSingleton<AudioController>
    {
        [Header("Settings")]
        [SerializeField] private AudioDatabaseSO audioDatabase;
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        private IAudioService _audioService;
        private IGoldService _currentMatchGoldService;

        protected override void Awake()
        {
            base.Awake();
            
            if (_audioService == null)
            {
                InitializeService();
                RegisterStaticEvents();
            }
        }

        private void Start()
        {
            PlayMusicForScene(SceneManager.GetActiveScene().name);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UnregisterStaticEvents();
            UnregisterMatchEvents();
        }
        
        public void PlayUISound(SoundKey key)
        {
            _audioService?.PlaySFX(key);
        }

        #region ---MATCH---
        public void ConnectToMatch(IGoldService goldService)
        {
            UnregisterMatchEvents();

            _currentMatchGoldService = goldService;
            if (_currentMatchGoldService != null)
            {
                //_currentMatchGoldService.OnGoldChanged += OnGoldChanged;
            }
        }
        
        public void DisconnectFromMatch()
        {
            UnregisterMatchEvents();
        }
        #endregion

        private void InitializeService()
        {
            AudioMixerGroup bgmGroup = audioMixer.FindMatchingGroups("Music")[0];
            AudioMixerGroup sfxGroup = audioMixer.FindMatchingGroups("SFX")[0];

            _audioService = new AudioService(audioDatabase, bgmSource, sfxSource, bgmGroup, sfxGroup);
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlayMusicForScene(scene.name);
        }

        private void PlayMusicForScene(string sceneName)
        {
            if (sceneName == "Menu" || sceneName == "Loading")
            {
                _audioService.PlayBGM(SoundKey.BGM_MainMenu);
            }
            else
            {
                _audioService.PlayBGM(SoundKey.BGM_Gameplay);
            }
        }
        
        private void RegisterStaticEvents()
        {
            ActionSystem.SubscribeReaction<AttackUA>(OnUnitAttack, UnitActionEventType.PRE);
            ActionSystem.SubscribeReaction<HealUA>(OnUnitHeal, UnitActionEventType.POST);
            //ActionSystem.SubscribeReaction<MoveUA>(OnUnitMove, UnitActionEventType.PRE);
        }
        
        private void UnregisterStaticEvents()
        {
             ActionSystem.UnsubscribeReaction<AttackUA>(OnUnitAttack, UnitActionEventType.PRE);
             ActionSystem.UnsubscribeReaction<HealUA>(OnUnitHeal, UnitActionEventType.POST);
             //ActionSystem.UnsubscribeReaction<MoveUA>(OnUnitMove, UnitActionEventType.PRE);
        }
        
        private void UnregisterMatchEvents()
        {
            if (_currentMatchGoldService != null)
            {
                //_currentMatchGoldService.OnGoldChanged -= OnGoldChanged;
                _currentMatchGoldService = null;
            }
        }

        #region ---HANDLERS---
        private void OnUnitAttack(BaseUnitAction action) 
        {
             var attack = (AttackUA) action;
             _audioService.PlaySFX(SoundKey.SFX_Attack);
             
             if(attack.IsDead) 
                 _audioService.PlaySFX(SoundKey.SFX_Die);
             else 
                 _audioService.PlaySFX(SoundKey.SFX_Hurt);
        }
        
        private void OnUnitHeal(BaseUnitAction action) => _audioService.PlaySFX(SoundKey.SFX_Heal);
        //private void OnUnitMove(BaseUnitAction action) => _audioService.PlaySFX(SoundKey.SFX_UnitMove);
        //private void OnGoldChanged(TeamColor team, int amount) => _audioService.PlaySFX(SoundKey.SFX_GetGold);
        #endregion
    }
}