using System.Text;
using Sandbox.ModAPI;

namespace NukeGuard_TractorBeam.TractorBeams.Controls
{
	public class StrengthSlider<T> : Slider<T>
	{
		private readonly TractorUi<T> _mUi;
		public StrengthSlider(
			TractorUi<T> ui,
			IMyTerminalBlock block,
			string internalName,
			string title,
			float min = 1.0f,
			float max = 10000.0f,
			float standard = 2000.0f,
			bool log = false)
			: base(block, internalName, title, min, max, standard, log)
		{
			_mUi = ui;
			CreateUi();
		}

		public override void Writer(IMyTerminalBlock block, StringBuilder builder)
		{
			builder.Clear();
			builder.Append(Getter(block) + " N");
		}

		public override void Setter(IMyTerminalBlock block, float value)
		{
			base.Setter(block, value);
			_mUi.Sync(block);
		}

		public void SetterNoCheck(IMyTerminalBlock block, float value)
		{
			base.Setter(block, value);
		}

	}
}