namespace MADP.Settings
{
    public enum TeamStyle
    {
        Normal,
        Attack,
        Defense,
        Econ
    }

    [System.Serializable]
    public struct TeamSetting
    {
        public int hp;
        public int atk;
        public int def;
    }
}