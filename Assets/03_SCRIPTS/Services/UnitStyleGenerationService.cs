using MADP.Models;
using MADP.Settings;

namespace MADP.Services
{
    public class UnitStyleGenerationService
    {
        public TeamStyle CreateTeamStyle(TeamColor teamColor)
        {
            switch (teamColor)
            {
                case TeamColor.Blue:
                    return TeamStyle.Defense;
                case TeamColor.Red:
                    return TeamStyle.Attack;
                case TeamColor.Yellow:
                    return TeamStyle.Econ;
                case TeamColor.Green:
                    return TeamStyle.Normal;
                default:
                    return TeamStyle.Normal;
            }
        }
    }
}