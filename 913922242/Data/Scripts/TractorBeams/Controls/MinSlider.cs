using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;

namespace NukeGuard_TractorBeam.TractorBeams.Controls
{
	public class MinSlider<T> : Slider<T>
	{
		private readonly TractorUi<T> _mUi;
		public MinSlider(
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

		public void SetterNoCheck(IMyTerminalBlock block, float value)
		{
			base.Setter(block, value);
		}

		public override void Setter(IMyTerminalBlock block, float value)
		{
			base.Setter(block, value);
			List<IMyTerminalControl> controls = new List<IMyTerminalControl>();
			MyAPIGateway.TerminalControls.GetControls<T>(out controls);
			IMyTerminalControl maxSlider = controls.Find((x) => x.Id == "MaxSlider" + Definition.SubtypeId);
			if (maxSlider != null && _mUi.MaxSlider != null)
			{
				float maxValue = _mUi.MaxSlider.Getter(block);
				_mUi.MaxSlider.SetterNoCheck(block, Math.Max(maxValue, value));
				maxSlider.UpdateVisual();
				_mUi.Sync(block);
			}
		}
	}
}