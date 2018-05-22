using ScorpionMonitor;

namespace Scene
{
	public static class SceneServerMonitor
	{
		public static readonly IScorpionMeter TickRate;
		public static readonly IScorpionCounter SceneTotalNumber;
		public static readonly IScorpionCounter PlayerNumber;
		public static readonly IScorpionCounter ObjNumber;
		public static readonly IScorpionMeter CreatedDropItemRate;
		public static readonly IScorpionMeter UseSkillRate;
		static SceneServerMonitor()
		{
			var contextName = "Scene[" + SceneServer.Instance.Id + "]";
			var impl = new ScorpionServerMonitor(contextName);

			TickRate = impl.Meter("TickRate",SMUnit.Calls);
			SceneTotalNumber = impl.Conter("SceneTotalNumber",SMUnit.Items);
			PlayerNumber = impl.Conter("PlayerNumber", SMUnit.Items);
			ObjNumber = impl.Conter("ObjNumber", SMUnit.Items);
			CreatedDropItemRate = impl.Meter("CreatedDropItemRate", SMUnit.Items);
			UseSkillRate = impl.Meter("UseSkillRate", SMUnit.Calls);
		}
		

	}
}
