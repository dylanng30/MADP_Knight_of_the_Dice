using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MADP.Models
{
    [Serializable]
    public class GameSettingsModel
    {
        // General Settings
        public int ResolutionIndex = 0;
        public bool IsFullScreen = true;

        //Sound Settings
        public float MasterVolume = 1.0f;
        public float MusicVolume = 1.0f;
        public float SfxVolume = 1.0f;
        
        //Trạng thái Mute cho từng kênh
        public bool IsMasterMuted = false;
        public bool IsMusicMuted = false;
        public bool IsSfxMuted = false;
    }
}
