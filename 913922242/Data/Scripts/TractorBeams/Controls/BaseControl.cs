using System.Collections.Generic;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.ObjectBuilders;

namespace NukeGuard_TractorBeam.TractorBeams.Controls
{
    public class BaseControl<T>
    {
        public SerializableDefinitionId Definition;
        public string InternalName;
        public string Title;

        public BaseControl(
            IMyTerminalBlock block,
            string internalName,
            string title)
        {
            Definition = block.BlockDefinition;
            InternalName = internalName + Definition.SubtypeId;
            Title = title;
        }

        public void CreateUi()
        {
            List<IMyTerminalControl> controls = new List<IMyTerminalControl>();
            MyAPIGateway.TerminalControls.GetControls<T>(out controls);
            IMyTerminalControl control = controls.Find((x) => x.Id.ToString() == InternalName);
            if (control == null)
            {
                OnCreateUi();
            }
        }

        public virtual void OnCreateUi()
        {
        }

        public virtual bool Enabled(IMyTerminalBlock block)
        {
            return true;
        }

        public virtual bool ShowControl(IMyTerminalBlock block)
        {
            return block.BlockDefinition.TypeId == Definition.TypeId &&
                   block.BlockDefinition.SubtypeId == Definition.SubtypeId;
        }
    }
}
