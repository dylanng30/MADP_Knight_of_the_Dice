using System;
using MADP.Models.VFX.Interfaces;
using UnityEngine;

namespace MADP.Views.VFX
{
    public abstract class BaseVFXView : MonoBehaviour
    {
        protected Action<BaseVFXView> _onCompleteCallback;
        
        public virtual void Setup(IVFXPayload payload)
        {
            
        }

        public virtual void Play(Vector3 position, Action<BaseVFXView> onComplete)
        {
            transform.position = position;
            _onCompleteCallback = onComplete;
            gameObject.SetActive(true);
        }
        
        public virtual void Stop()
        {
            ReturnToPool();
        }

        protected void ReturnToPool()
        {
            gameObject.SetActive(false);
            _onCompleteCallback?.Invoke(this);
        }
    }
}