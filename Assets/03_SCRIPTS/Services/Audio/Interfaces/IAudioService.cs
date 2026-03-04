using MADP.Settings;

namespace MADP.Services.Audio.Interfaces
{
    public interface IAudioService
    {
        void PlayBGM(SoundKey key);
        void PlaySFX(SoundKey key);
        void StopBGM();
    }
}