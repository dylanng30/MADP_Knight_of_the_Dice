using System;
using System.Collections.Generic;
using UnityEngine;

namespace MADP.Settings
{
    [Serializable]
    public struct TeamColorSetting
    {
        public Color Red;
        public Color Yellow;
        public Color Purple;
        public Color Blue;
        public Color Green;
        public Color DefaultColor;
    }
    [CreateAssetMenu(fileName = "TeamColorPalette", menuName = "MADP/Settings/Team Color Palette")]
    public class ColorPaletteSetting : ScriptableObject
    {
        public TeamColorSetting TeamColorSetting;
        public List<MapSetting> MapSettings = new List<MapSetting>();
    }
}