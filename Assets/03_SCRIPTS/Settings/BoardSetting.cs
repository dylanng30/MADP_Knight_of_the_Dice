using System;
using UnityEngine;

namespace MADP.Settings
{
    [Serializable]
    public struct BoardSettings
    {
        [Header("--- SPEACIAL CELL SETTING ---")]
        public int RedCellCount;
        public int YellowCellCount;
        public int PurpleCellCount;
    }
}