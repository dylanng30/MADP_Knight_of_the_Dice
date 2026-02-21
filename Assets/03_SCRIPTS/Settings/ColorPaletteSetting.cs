using System.Collections.Generic;
using UnityEngine;

namespace MADP.Settings
{
    [CreateAssetMenu(fileName = "TeamColorPalette", menuName = "MADP/Settings/Team Color Palette")]
    public class ColorPaletteSetting : ScriptableObject
    {
        public TeamColorSetting TeamColorSetting;
        public List<MapSetting> MapSettings = new List<MapSetting>();
    }
}