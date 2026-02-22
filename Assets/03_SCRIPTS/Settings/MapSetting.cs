using System;
using UnityEngine;

namespace MADP.Settings
{
    public enum MapType
    {
        Desert, Snow
    }
    [Serializable]
    public struct MapSetting
    {
        public MapType MapType;
        public Color PrimaryColor;
        public Color SecondaryColor;
    }
}