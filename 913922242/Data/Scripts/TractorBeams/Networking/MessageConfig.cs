using System.Collections.Generic;
using ProtoBuf;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.ModAPI;

namespace NukeGuard_TractorBeam.TractorBeams.Networking
{
	[ProtoContract]
	public class MessageConfig : OrigMessageBase
	{
		[ProtoMember(1)]
		public long EntityId;

		[ProtoMember(2)]
		public float Max = 15; // meters

		[ProtoMember(3)]
		public float Min = 10; // meters

		[ProtoMember(4)]
		public float Strength = 50;  // Newton

		public override void ProcessClient()
		{
			Proc();
		}

		public override void ProcessServer()
		{
			Proc();
			// None
		}


		public void Proc()
		{
			List<IMyTerminalControl> controls = new List<Sandbox.ModAPI.Interfaces.Terminal.IMyTerminalControl>();

			IMyEntity block;
			if (MyAPIGateway.Entities.TryGetEntityById(EntityId, out block))
			{
				TractorBeamCore traction = block.GameLogic.GetAs<TractorBeamCore>();

				traction.Ui.MinSlider.SetterNoCheck((IMyTerminalBlock)block, Min);
				traction.Ui.MaxSlider.SetterNoCheck((IMyTerminalBlock)block, Max);
				traction.Ui.StrengthSlider.SetterNoCheck((IMyTerminalBlock)block, Strength);

				MyAPIGateway.TerminalControls.GetControls<Sandbox.ModAPI.Ingame.IMyLargeTurretBase>(out controls);
				foreach (IMyTerminalControl control in controls)
				{
					control.UpdateVisual();
				}
			}
		}
	}
}