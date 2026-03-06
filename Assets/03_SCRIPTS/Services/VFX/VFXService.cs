using System.Collections.Generic;
using MADP.Models;
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
        
        private Dictionary<VFXType, Queue<VFXView>> _vfxPool = new();

        public VFXService(VFXDatabaseSO database)
        {
            _database = database;
        }

        public void Initialize(Transform poolContainer)
        {
            _poolContainer = poolContainer;
        }

        public void PlayVFX(VFXType type, Vector3 position)
        {
            VFXView vfx = GetFromPool(type);
            if (vfx != null)
            {
                vfx.Play(position, (finishedVfx) => ReturnToPool(type, finishedVfx));
            }
        }

        private VFXView GetFromPool(VFXType type)
        {
            if (!_vfxPool.ContainsKey(type))
            {
                _vfxPool[type] = new Queue<VFXView>();
            }
            
            if (_vfxPool[type].Count > 0)
            {
                return _vfxPool[type].Dequeue();
            }
            
            VFXView prefab = _database.GetPrefab(type);
            if (prefab == null)
            {
                Debug.LogWarning($"[VFXService] Khong tim thay prefab cho loai {type}");
                return null;
            }

            VFXView newVfx = Object.Instantiate(prefab, _poolContainer);
            newVfx.gameObject.SetActive(false);
            return newVfx;
        }

        private void ReturnToPool(VFXType type, VFXView vfx)
        {
            vfx.gameObject.SetActive(false);
            _vfxPool[type].Enqueue(vfx);
        }
    }
}