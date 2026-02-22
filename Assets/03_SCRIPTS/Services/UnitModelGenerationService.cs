using System;
using System.Collections.Generic;
using MADP.Models;

namespace MADP.Services
{
    public class UnitModelGenerationService
    {
        public Dictionary<TeamColor, List<UnitModel>> CreateAllUnits(List<TeamColor> activeTeams)
        {
            var allUnits = new Dictionary<TeamColor, List<UnitModel>>();
            
            foreach (var teamColor in activeTeams)
            {
                allUnits.Add(teamColor, CreateTeamUnit(teamColor));
            }   
            
            return allUnits;
        }

        private List<UnitModel> CreateTeamUnit(TeamColor teamColor)
        {
            List<UnitModel> teamUnits = new List<UnitModel>();
            for(int i = 0; i < 4; i++)
            {
                UnitStatModel statModel = new UnitStatModel(5, 2, 1);
                UnitModel model = new UnitModel(i, teamColor, statModel);
                teamUnits.Add(model);
            }
            
            return teamUnits;
        }
    }
}