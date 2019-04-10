using ProtoBuf;
using Sandbox.ModAPI;

namespace NukeGuard_TractorBeam.TractorBeams.Networking
{
	[ProtoContract]
	public class MessageDebug : MessageBase
	{
		[ProtoMember(1)]
		public string Text;

		public override void ProcessClient()
		{
			MyAPIGateway.Utilities.ShowNotification(Text, 6000);
		}

		public override void ProcessServer()
		{
			// None
		}

	}
}