namespace NukeGuard_TractorBeam.TractorBeams
{
	public class AttractorWeaponInfo
	{
		public float PowerUsage;
		public float Damage;

		public string AmmoName;
		public int Classes;

		public float MaxHeat;
		public float HeatPerTick;
		public float HeatDissipationPerTick;
		public int HeatDissipationDelay;
		
		public int KeepAtCharge;

		public AttractorWeaponInfo (float powerUsage, float damage, string ammoName, int classes, float maxHeat, float heatPerTick, float heatDissipationPerTick, int heatDissipationDelay, int keepAtCharge) {

			PowerUsage = powerUsage;
			Damage = damage;
			AmmoName = ammoName;
			Classes = classes;
			MaxHeat = maxHeat;
			HeatPerTick = heatPerTick;
			HeatDissipationPerTick = heatDissipationPerTick;
			HeatDissipationDelay = heatDissipationDelay;
			KeepAtCharge = keepAtCharge;
		}
	}
	

}

