using Sandbox.ModAPI;

namespace NukeGuard_TractorBeam.TractorBeams.Controls
{
	public class SliderAction<T> : ControlAction<T>
	{
		readonly Slider<T> _slider;
		readonly float _incPerAction;

		public SliderAction(
			IMyTerminalBlock block,
			string internalName,
			string name,
			string icon,
			Slider<T> slider,
			float incPerAction)
			: base(block, internalName, name, icon)
		{
			_slider = slider;
			_incPerAction = incPerAction;
		}

		public override void OnAction(IMyTerminalBlock block)
		{
			if (_slider.Log)
			{
				_slider.Setter(block, _slider.Getter(block) * _incPerAction);
			}
			else
			{
				_slider.Setter(block, _slider.Getter(block) + _incPerAction);
			}
		}
	}
}