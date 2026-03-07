using System;
using System.Collections;
using MADP.Models.VFX;
using MADP.Models.VFX.Interfaces;
using TMPro;
using UnityEngine;

namespace MADP.Views.VFX
{
    public class TextVFXView : BaseVFXView
    {
        [SerializeField] private TMP_Text _floatingText;

        [Header("SETTINGS")] 
        [SerializeField] private float floatingTime = 1f;
        
        public override void Setup(IVFXPayload payload)
        {
            base.Setup(payload);
            
            if (payload is NumberVFXPayload numberPayload)
            {
                if (_floatingText != null)
                {
                    _floatingText.text = $"{numberPayload.Prefix}{numberPayload.Value}";
                    _floatingText.color = numberPayload.TextColor;
                }
            }
        }

        public override void Play(Vector3 position, Action<BaseVFXView> onComplete)
        {
            base.Play(position, onComplete);

            StartCoroutine(Floating());
        }

        private IEnumerator Floating()
        {
            float timer = 0;
            while (timer <= floatingTime)
            {
                transform.position += Vector3.up * 2 * Time.deltaTime / floatingTime;
                transform.LookAt(Camera.main.transform.position);
                timer += Time.deltaTime;
                yield return null;
            }
            
            ReturnToPool();
        }
    }
}