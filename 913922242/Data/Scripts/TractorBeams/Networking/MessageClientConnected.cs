using ProtoBuf;

namespace NukeGuard_TractorBeam.TractorBeams.Networking
{
	[ProtoContract]
	public class MessageClientConnected : MessageBase
	{

		public override void ProcessClient()
		{
		}

		public override void ProcessServer()
		{
			//var messageOut = configPair.Value;
			//MessageUtils.SendMessageToPlayer(SenderSteamId, (MessageBase)messageOut);
		}
	}
}