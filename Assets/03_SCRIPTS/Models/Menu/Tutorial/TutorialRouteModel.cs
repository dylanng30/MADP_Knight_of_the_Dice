using System;
using System.Collections.Generic;

namespace MADP.Models.Menu.Tutorial
{
    [Serializable]
    public class TutorialRouteModel
    {
        public List<TutorialStepModel> Steps;
        
        public TutorialStepModel GetStep(int index)
        {
            if (index >= 0 && index < Steps.Count)
                return Steps[index];
            return null;
        }
    }
}