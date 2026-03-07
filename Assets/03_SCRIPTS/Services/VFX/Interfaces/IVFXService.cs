using MADP.Models.VFX.Interfaces;
using MADP.Settings;
using UnityEngine;

namespace MADP.Services.VFX.Interfaces
{
    public interface IVFXService
    {
        void Initialize(Transform poolContainer);
        void PlayVFX(VFXType type, Vector3 position, IVFXPayload payload = null);
    }
}