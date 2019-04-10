using NukeGuard_TractorBeam.TractorBeams.Controls;
using NukeGuard_TractorBeam.TractorBeams.Networking;
using Sandbox.ModAPI;

namespace NukeGuard_TractorBeam.TractorBeams
{
    public class TractorUi<T>
    {
        public MaxSlider<T> MaxSlider;
        public MinSlider<T> MinSlider;
        public StrengthSlider<T> StrengthSlider;

        public bool Initialized = false;
        public void CreateUi(IMyTerminalBlock block)
        {
            if (Initialized) { return; }
            Initialized = true;

            MinSlider = new MinSlider<T>(this, 
                block,
              "MinSlider",
              "Minimum Distance",
              3, 200, 50);
            MaxSlider = new MaxSlider<T>(this,
                block,
              "MaxSlider",
              "Maximum Distance",
              3, 200, 60);
            StrengthSlider = new StrengthSlider<T>(this,
                block,
              "StrengthSlider",
              "Strength",
              2000, 990000, 10000, true);

            new SliderAction<T>(block, "IncMax", "Increase maximum distance", "", MaxSlider, 1.0f);
            new SliderAction<T>(block, "DecMax", "Decrease maximum distance", "", MaxSlider, -1.0f);
            new SliderAction<T>(block, "IncMin", "Increase minimum distance", "", MinSlider, 1.0f);
            new SliderAction<T>(block, "DecMin", "Decrease minimum distance", "", MinSlider, -1.0f);
            new SliderAction<T>(block, "IncStr", "Increase strength", "", StrengthSlider, 1.1f);
            new SliderAction<T>(block, "DecStr", "Decrease strength", "", StrengthSlider, 0.9f);

        }

        public void Sync(IMyTerminalBlock block)
        {
            MessageConfig msg = new MessageConfig();
            msg.EntityId = block.EntityId;
            msg.Min = MinSlider.Getter(block);
            msg.Max = MaxSlider.Getter(block);
            msg.Strength = StrengthSlider.Getter(block);
            MessageUtils.SendMessageToAll(msg);
        }
    }
}
