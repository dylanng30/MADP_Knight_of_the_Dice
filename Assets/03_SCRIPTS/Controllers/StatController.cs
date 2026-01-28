using System;
using System.Collections.Generic;
using System.Linq;
using _03_SCRIPTS.SO;
using MADP.Models;
using MADP.Models.UnitActions;
using MADP.Services;
using MADP.Settings;
using MADP.Systems;
using UnityEngine;

namespace MADP.Controllers
{
    public class StatController : MonoBehaviour
    {
        [SerializeField] private BoardController _boardController;
        [SerializeField] private TeamData[] _teamData = new TeamData[4];
        private UnitStyleGenerationService _unitStyleGenerationService = new();

        private Dictionary<TeamColor, List<UnitModel>> _allUnits = new();
        private Dictionary<UnitModel, StatModel> _statMapper = new();
        private Dictionary<TeamColor, TeamData> _teamDataMapper = new();

        private void Start()
        {
            CreateTeamData();
            CreateStatMapper();
        }

        public void DamageToTarget(UnitModel dealer, UnitModel target)
        {
            StatModel dealerStat = _statMapper[dealer];
            StatModel targetStat = _statMapper[target];
            DealDamageUA damageUa = new DealDamageUA(target, targetStat, dealerStat.DMG);
            ActionSystem.Instance.Perform(damageUa);
        }

        void CreateStatMapper()
        {
            //_allUnits = _getAllUnitService.GetAllUnits();
            _allUnits = _boardController.AllUnits;

            foreach (var team in _allUnits)
            {
                TeamColor color = team.Key;
                List<UnitModel> units = team.Value;
                // CreateTeamDataMapper(color);
                TeamData teamData = _teamDataMapper[color];
                for (int i = 0; i < units.Count; i++)
                {
                    TeamSetting teamSetting = teamData.settings[i];
                    StatModel stat = new StatModel(teamSetting.atk, teamSetting.hp, teamSetting.def);
                    _statMapper.Add(units[i], stat);
                    //Debug.Log($"Color: {color} - {stat.DMG} - {stat.HP} - {stat.DEF}");
                }
            }
        }

        void CreateTeamDataMapper(TeamColor color)
        {
            TeamStyle style = _unitStyleGenerationService.CreateTeamStyle(color);
            TeamData teamData = _teamData.FirstOrDefault(x => x.teamStyle == style);
            _teamDataMapper.Add(color, teamData);
            
            foreach (var so in _teamData)
            {

                // switch (color)
                // {
                //     case TeamColor.Red:
                //         _teamDataMapper.Add(color, teamData);
                //         break;
                //     case TeamColor.Blue:
                //         _teamDataMapper.Add(color, teamData);
                //         break;
                //     case TeamColor.Green:
                //         _teamDataMapper.Add(color, teamData);
                //         break;
                //     case TeamColor.Yellow:
                //         _teamDataMapper.Add(color, teamData);
                //         break;
                // }
            }
        }

        void CreateTeamData()
        {
            CreateTeamDataMapper(TeamColor.Red);
            CreateTeamDataMapper(TeamColor.Blue);
            CreateTeamDataMapper(TeamColor.Green);
            CreateTeamDataMapper(TeamColor.Yellow);
        }
    }
}