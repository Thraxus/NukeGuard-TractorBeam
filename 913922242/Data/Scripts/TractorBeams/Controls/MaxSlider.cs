using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;

namespace NukeGuard_TractorBeam.TractorBeams.Controls
{
	public class MaxSlider<T> : Slider<T>
	{
		private readonly TractorUi<T> _mUi;
		public MaxSlider(
			TractorUi<T> ui,
			IMyTerminalBlock block,
			string internalName,
			string title,
			float min = 0.0f,
			float max = 100.0f,
			float standard = 10.0f)
			: base(block, internalName, title, min, max, standard)
		{
			_mUi = ui;
			CreateUi();
		}

		public override void Writer(IMyTerminalBlock block, StringBuilder builder)
		{
			builder.Clear();
			builder.Append(Getter(block) + " m");
		}

		public override void Setter(IMyTerminalBlock block, float value)
		{
			base.Setter(block, value);
			List<IMyTerminalControl> controls = new List<IMyTerminalControl>();
			MyAPIGateway.TerminalControls.GetControls<T>(out controls);
			IMyTerminalControl minSlider = controls.Find((x) => x.Id == "MinSlider" + Definition.SubtypeId);
			if (minSlider == null || _mUi.MinSlider == null) return;
			float minValue = _mUi.MinSlider.Getter(block);
			_mUi.MinSlider.SetterNoCheck(block, Math.Min(minValue, value));
			minSlider.UpdateVisual();
			_mUi.Sync(block);
		}

		public void SetterNoCheck(IMyTerminalBlock block, float value)
		{
			base.Setter(block, value);
		}
	}
}