﻿/*
Copyright © 2016 Leto
This work is free. You can redistribute it and/or modify it under the
terms of the Do What The Fuck You Want To Public License, Version 2,
as published by Sam Hocevar. See http://www.wtfpl.net/ for more details.
*/

using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.ObjectBuilders;

namespace NukeGuard_TractorBeam.TractorBeams.Controls
{
    public class ControlAction<T>
    {
        public SerializableDefinitionId Definition;
        public string InternalName;
        public string Name;

        public ControlAction(
            IMyTerminalBlock block,
            string internalName,
            string name,
            string icon)
        {
            Name = name;
            Definition = block.BlockDefinition;
            InternalName = internalName + Definition.SubtypeId;

            List<IMyTerminalAction> controls = new List<IMyTerminalAction>();
            MyAPIGateway.TerminalControls.GetActions<T>(out controls);
            IMyTerminalAction control = controls.Find((x) => x.Id.ToString() == InternalName);
            if (control == null)
            {
                IMyTerminalAction action = MyAPIGateway.TerminalControls.CreateAction<T>(InternalName);
                action.Action = OnAction;
                action.Name = new StringBuilder(Name);
                action.Enabled = Visible;
                action.Writer = Writer;
                action.Icon = icon;
                MyAPIGateway.TerminalControls.AddAction<T>(action);
            }
        }

        public virtual void Writer(IMyTerminalBlock block, StringBuilder builder)
        {
        
        }

        public virtual void OnAction(IMyTerminalBlock block)
        {
        }

        public virtual bool Visible(IMyTerminalBlock block)
        {
            return block.BlockDefinition.TypeId == Definition.TypeId &&
                    block.BlockDefinition.SubtypeId == Definition.SubtypeId;
        }
    }
}
