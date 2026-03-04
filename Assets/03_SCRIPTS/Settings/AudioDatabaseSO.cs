using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MADP.Settings
{
    public enum SoundKey
    {
        //BGM
        BGM_MainMenu = 0, BGM_Gameplay = 1,
        
        //SFX
        SFX_ButtonClick = 20, SFX_DiceRoll = 21,SFX_Attack = 22, SFX_Die = 23,
        SFX_Heal = 24, SFX_Hurt = 25, SFX_BuySuccess = 26,
    }

    [Serializable]
    public struct SoundData
    {
        public SoundKey Key;
        public AudioClip Clip;
        [Range(0f, 1f)] public float VolumeScale;
    }
    
    [CreateAssetMenu(fileName = "AudioDatabase", menuName = "MADP/Settings/Audio Database")]
    public class AudioDatabaseSO : ScriptableObject
    {
        public List<SoundData> Sounds;
        private Dictionary<SoundKey, SoundData> _lookup;
        
        public void Initialize()
        {
            _lookup = Sounds.ToDictionary(x => x.Key, x => x);
        }
        
        public bool TryGetClip(SoundKey key, out AudioClip clip, out float volumeScale)
        {
            if (_lookup == null) Initialize();

            if (_lookup.TryGetValue(key, out var data))
            {
                clip = data.Clip;
                volumeScale = data.VolumeScale > 0 ? data.VolumeScale : 1f;
                return true;
            }

            clip = null;
            volumeScale = 1f;
            return false;
        }
    }
}