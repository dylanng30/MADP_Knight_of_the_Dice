using MADP.Controllers;
using MADP.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Utilities
{
    [RequireComponent(typeof(Button))]
    public class PlaySoundOnClick : MonoBehaviour
    {
        [SerializeField] private SoundKey soundKey = SoundKey.SFX_ButtonClick;

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(PlaySound);
        }

        private void PlaySound()
        {
            if (AudioController.Instance != null) 
                AudioController.Instance.PlayUISound(soundKey);
        }
    }
}