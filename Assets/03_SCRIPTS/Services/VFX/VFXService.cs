using System.Collections.Generic;
using MADP.Models;
using MADP.Models.VFX.Interfaces;
using MADP.Services.VFX.Interfaces;
using MADP.Settings;
using MADP.Views.VFX;
using UnityEngine;

namespace MADP.Services.VFX
{
    public class VFXService : IVFXService
    {
        private VFXDatabaseSO _database;
        private Transform _poolContainer;
        
        private Dictionary<VFXType, Queue<BaseVFXView>> _vfxPool = new();

        public VFXService(VFXDatabaseSO database)
        {
            _database = database;
        }

        public void Initialize(Transform poolContainer)
        {
            _poolContainer = poolContainer;
        }

        public void PlayVFX(VFXType type, Vector3 position, IVFXPayload payload = null)
        {
            BaseVFXView vfx = GetFromPool(type);
            if (vfx != null)
            {
                vfx.Setup(payload);
                vfx.Play(position, (finishedVfx) => ReturnToPool(type, finishedVfx));
            }
        }

        private BaseVFXView GetFromPool(VFXType type)
        {
            if (!_vfxPool.ContainsKey(type))
            {
                _vfxPool[type] = new Queue<BaseVFXView>();
            }
            
            if (_vfxPool[type].Count > 0)
            {
                return _vfxPool[type].Dequeue();
            }
            
            BaseVFXView prefab = _database.GetPrefab(type);
            if (prefab == null)
            {
                Debug.LogWarning($"[VFXService] Khong tim thay prefab cho loai {type}");
                return null;
            }

            BaseVFXView newVfx = Object.Instantiate(prefab, _poolContainer);
            newVfx.gameObject.SetActive(false);
            return newVfx;
        }

        private void ReturnToPool(VFXType type, BaseVFXView vfx)
        {
            vfx.gameObject.SetActive(false);
            _vfxPool[type].Enqueue(vfx);
        }
    }
}