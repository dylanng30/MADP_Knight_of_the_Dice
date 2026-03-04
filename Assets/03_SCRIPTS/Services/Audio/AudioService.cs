using MADP.Services.Audio.Interfaces;
using MADP.Settings;
using UnityEngine;
using UnityEngine.Audio;

namespace MADP.Services.Audio
{
    public class AudioService : IAudioService
    {
        private readonly AudioDatabaseSO _database;
        private readonly AudioSource _bgmSource;
        private readonly AudioSource _sfxSource;

        public AudioService(AudioDatabaseSO database, AudioSource bgmSource, AudioSource sfxSource, AudioMixerGroup bgmGroup, AudioMixerGroup sfxGroup)
        {
            _database = database;
            _database.Initialize();

            _bgmSource = bgmSource;
            _bgmSource.outputAudioMixerGroup = bgmGroup;
            _bgmSource.loop = true;

            _sfxSource = sfxSource;
            _sfxSource.outputAudioMixerGroup = sfxGroup;
        }

        public void PlayBGM(SoundKey key)
        {
            if (_database.TryGetClip(key, out var clip, out var volume))
            {
                if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;

                _bgmSource.clip = clip;
                _bgmSource.volume = volume;
                _bgmSource.Play();
            }
            else
            {
                Debug.LogWarning($"AudioClip not found for BGM: {key}");
            }
        }

        public void PlaySFX(SoundKey key)
        {
            if (_database.TryGetClip(key, out var clip, out var volume))
            {
                _sfxSource.PlayOneShot(clip, volume);
            }
            else
            {
                Debug.LogWarning($"AudioClip not found for SFX: {key}");
            }
        }

        public void StopBGM()
        {
            _bgmSource.Stop();
        }
    }
}