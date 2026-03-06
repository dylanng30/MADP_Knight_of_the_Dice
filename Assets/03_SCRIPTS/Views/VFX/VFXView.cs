using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MADP.Views.VFX
{
    public class VFXView : MonoBehaviour
    {
        [SerializeField] private List<ParticleSystem>  _particleSystems;
        
        private Action<VFXView> _onCompleteCallback;

        public void Play(Vector3 position, Action<VFXView> onComplete)
        {
            transform.position = position;
            _onCompleteCallback = onComplete;
            gameObject.SetActive(true);
            
            
            
            if (_particleSystems != null)
            {
                foreach (var ps in _particleSystems)
                {
                    ps.Play();
                    StartCoroutine(WaitAndReturn(ps.main.duration));
                }
            }
        }

        public void Stop()
        {
            if (_particleSystems != null)
            {
                foreach (var ps in _particleSystems)
                {
                    ps.Stop();
                }
            }
            ReturnToPool();
        }

        private IEnumerator WaitAndReturn(float duration)
        {
            yield return new WaitForSeconds(duration);
            ReturnToPool();
        }

        private void ReturnToPool()
        {
            gameObject.SetActive(false);
            _onCompleteCallback?.Invoke(this);
        }
    }
}