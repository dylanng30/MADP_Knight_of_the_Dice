using System;
using UnityEngine;

namespace MADP.Settings
{
    [Serializable]
    public struct CellMaterialSetting
    {
        public Material RedSpawn;
        public Material BlueSpawn;
        public Material YellowSpawn;
        public Material GreenSpawn;
        [Space(5)]
        public Material RedHome;
        public Material BlueHome;
        public Material YellowHome;
        public Material GreenHome;
        [Space(5)]
        public Material Normal;
        public Material Red;
        public Material Yellow;
        public Material Purple;
    }
}