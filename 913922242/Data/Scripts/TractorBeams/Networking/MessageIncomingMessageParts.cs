using System.Linq;
using ProtoBuf;

namespace NukeGuard_TractorBeam.TractorBeams.Networking
{
	[ProtoContract]
	public class MessageIncomingMessageParts : OrigMessageBase
	{
		[ProtoMember(1)]
		public byte[] Content;

		[ProtoMember(2)]
		public bool LastPart;

		public override void ProcessClient()
		{
			MessageUtils.ClientMessageCache.AddRange(Content.ToList());

			if (LastPart)
			{
				MessageUtils.HandleMessage(MessageUtils.ClientMessageCache.ToArray());
				MessageUtils.ClientMessageCache.Clear();
			}
		}

		public override void ProcessServer()
		{
			if (MessageUtils.ServerMessageCache.ContainsKey(SenderSteamId))
				MessageUtils.ServerMessageCache[SenderSteamId].AddRange(Content.ToList());
			else
				MessageUtils.ServerMessageCache.Add(SenderSteamId, Content.ToList());

			if (LastPart)
			{
				MessageUtils.HandleMessage(MessageUtils.ServerMessageCache[SenderSteamId].ToArray());
				MessageUtils.ServerMessageCache[SenderSteamId].Clear();
			}
		}

	}
}