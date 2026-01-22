namespace MADP.Models.UnitActions
{
    public class HealUA : BaseUnitAction
    {
        public UnitModel UnitModel { get; private set; }
        public int Amount { get; private set; }
        
        public HealUA(UnitModel unitModel, int amount)
        {
            UnitModel = unitModel;
            Amount = amount;
        }
    }
}