/*
Code shameless stolen from Phoenix and Shaostoal Laserdrill Mod (who got it from midspaces admin helper). 
I asked phoenix and I am allowed to use it.
*/

using System;
using NukeGuard_TractorBeam.TractorBeams.Networking;
using NukeGuard_TractorBeam.TractorBeams.Utilities;
using Sandbox.ModAPI;
using VRage.Game.Components;

namespace NukeGuard_TractorBeam.TractorBeams
{
    #region MP messaging

    #endregion

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    class NetworkSessionTractorBeam : MySessionComponentBase
    {
        bool _isInitialized = false;

        protected override void UnloadData()
        {
            try
            {
                MyAPIGateway.Multiplayer.UnregisterMessageHandler(MessageUtils.MessageId, MessageUtils.HandleMessage);
            }
            catch (Exception ex) { LseLogger.Instance.LogException(ex); }

            base.UnloadData();
        }

        public override void UpdateBeforeSimulation()
        {
            if (!_isInitialized && MyAPIGateway.Session != null)
                Init();
        }

        private void Init()
        {
            _isInitialized = true;
            MyAPIGateway.Multiplayer.RegisterMessageHandler(MessageUtils.MessageId, MessageUtils.HandleMessage);
        }
    }


    #region Message Splitting

    #endregion
}
// vim: tabstop=4 expandtab shiftwidth=4 nobackup
