using System;

namespace MADP.Models
{
    public enum PlayerType
    {
        Empty, Human, Bot
    }

    public enum RoleType
    {
        Random, Attacker, Defender, Speller, Miner
    }
    [Serializable]
    public class LobbySlotModel
    {
        public int SlotIndex;
        public TeamColor TeamColor;
        public PlayerType PlayerType;
        public RoleType RoleType;
        public string PlayerName;
        public int AvatarId;
        public bool IsHost = false;
        
        public bool HasPlayer => PlayerType != PlayerType.Empty;

        public LobbySlotModel(int index, TeamColor color)
        {
            SlotIndex = index;
            TeamColor = color;
            PlayerType = PlayerType.Empty;
            PlayerName = "Empty";
            RoleType = RoleType.Random;
        }
    }
}