using MADP.Models.VFX.Interfaces;
using UnityEngine;

namespace MADP.Models.VFX
{
    public class NumberVFXPayload : IVFXPayload
    {
        public int Value { get; private set; }
        public Color TextColor { get; private set; }
        public string Prefix { get; private set; }

        public NumberVFXPayload(int value, Color color, string prefix = "")
        {
            Value = value;
            TextColor = color;
            Prefix = prefix;
        }
    }
}