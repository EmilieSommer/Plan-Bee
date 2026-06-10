public static class EggTypeExtensions
{
    public static Bee.BeeType ToBeeType(this EggType e)
    {
        switch (e)
        {
            case EggType.Builder: return Bee.BeeType.Builder;
            case EggType.Nurse:   return Bee.BeeType.Nurse;
            case EggType.House:   return Bee.BeeType.House;
            case EggType.Forager: return Bee.BeeType.Forager;
            case EggType.Drone:   return Bee.BeeType.Drone;
            default:              return Bee.BeeType.Forager;
        }
    }
}
