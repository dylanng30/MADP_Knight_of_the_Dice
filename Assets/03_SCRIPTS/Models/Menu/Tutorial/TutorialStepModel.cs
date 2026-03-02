using System;
using UnityEngine;

namespace MADP.Models.Menu.Tutorial
{
    [Serializable]
    public class TutorialStepModel
    {
        public bool IsOpened;
        public string TutorialMap;
        public string Description;
        public Sprite MapAvatar;
    }
}