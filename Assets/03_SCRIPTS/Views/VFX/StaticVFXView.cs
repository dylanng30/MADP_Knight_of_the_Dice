using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MADP.Views.VFX
{
    public class StaticVFXView : BaseVFXView
    {
        [SerializeField] private List<ParticleSystem>  _particleSystems;
        
        public override void Play(Vector3 position, Action<BaseVFXView> onComplete)
        {
            base.Play(position, onComplete);
            
            if (_particleSystems != null)
            {
                foreach (var ps in _particleSystems)
                {
                    ps.Play();
                    StartCoroutine(WaitAndReturn(ps.main.duration));
                }
            }
        }

        public override void Stop()
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
    }
}