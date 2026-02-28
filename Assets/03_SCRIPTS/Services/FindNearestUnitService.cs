using System.Collections.Generic;
using MADP.Models;

namespace MADP.Services
{
    public struct NearestUnitResult
    {
        public UnitModel Unit;
        public int Distance;
        public bool Found;
    }

    public class FindNearestUnitService
    {
        public NearestUnitResult FindNearestEnemyForward(List<CellModel> cells, int currentIndex, TeamColor myTeam)
        {
            int total = cells.Count;

            for (int step = 1; step < total; step++)
            {
                int nextIndex = (currentIndex + step) % total;
                var cell = cells[nextIndex];

                if (cell.HasUnit && cell.Unit.TeamOwner != myTeam)
                {
                    return new NearestUnitResult
                    {
                        Unit = cell.Unit,
                        Distance = step,
                        Found =  true
                    };
                }
            }

            return new NearestUnitResult
            {
                Unit = null,
                Distance = -1,
                Found = false
            };
        }

        public NearestUnitResult FindNearestEnemyBackward(List<CellModel> cells, int currentIndex, TeamColor myTeam)
        {
            int total = cells.Count;

            for (int step = 1; step < total; step++)
            {
                int prevIndex = (currentIndex - step + total) % total;
                var cell = cells[prevIndex];

                if (cell.HasUnit && cell.Unit.TeamOwner != myTeam)
                {
                    return new NearestUnitResult
                    {
                        Unit = cell.Unit,
                        Distance = step,
                        Found = true
                    };
                }
            }

            return new NearestUnitResult
            {
                Unit = null,
                Distance = -1,
                Found = false
            };
        }

        public NearestUnitResult FindNearestUnitForward(List<CellModel> cells, int currentIndex)
        {
            int total = cells.Count;

            for (int step = 1; step < total; step++)
            {
                int nextIndex = (currentIndex + step) % total;
                var cell = cells[nextIndex];

                if (cell.HasUnit)
                {
                    return new NearestUnitResult
                    {
                        Unit = cell.Unit,
                        Distance = step,
                        Found = true
                    };
                }
            }

            return new NearestUnitResult
            {
                Unit = null,
                Distance = -1,
                Found = false
            };
        }
    }
}