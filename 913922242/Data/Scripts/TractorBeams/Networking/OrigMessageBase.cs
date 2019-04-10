using System;
using System.Xml.Serialization;
using NukeGuard_TractorBeam.TractorBeams.Utilities;
using ProtoBuf;

namespace NukeGuard_TractorBeam.TractorBeams.Networking
{
	/// <summary>
	/// This is a base class for all messages
	/// </summary>
	// ALL CLASSES DERIVED FROM MessageBase MUST BE ADDED HERE
	[XmlInclude(typeof(MessageIncomingMessageParts))]
	[XmlInclude(typeof(MessageClientConnected))]
	[XmlInclude(typeof(MessageConfig))]
	[XmlInclude(typeof(MessageDebug))]

	[ProtoContract]
	public abstract class OrigMessageBase
	{
		/// <summary>
		/// The SteamId of the message's sender. Note that this will be set when the message is sent, so there is no need for setting it otherwise.
		/// </summary>
		[ProtoMember(1)]
		public ulong SenderSteamId;

		/// <summary>
		/// Defines on which side the message should be processed. Note that this will be set when the message is sent, so there is no need for setting it otherwise.
		/// </summary>
		[ProtoMember(2)]
		public MessageSide Side = MessageSide.ClientSide;

		/*
        [ProtoAfterDeserialization]
        void InvokeProcessing() // is not invoked after deserialization from xml
        {
            Logger.Debug("START - Processing");
            switch (Side)
            {
                case MessageSide.ClientSide:
                    ProcessClient();
                    break;
                case MessageSide.ServerSide:
                    ProcessServer();
                    break;
            }
            Logger.Debug("END - Processing");
        }
        */

		public void InvokeProcessing()
		{
			switch (Side)
			{
				case MessageSide.ClientSide:
					InvokeClientProcessing();
					break;
				case MessageSide.ServerSide:
					InvokeServerProcessing();
					break;
			}
		}

		private void InvokeClientProcessing()
		{
			LseLogger.Instance.LogDebug(string.Format("START - Processing [Client] {0}", GetType().Name));
			try
			{
				ProcessClient();
			}
			catch (Exception ex)
			{
				LseLogger.Instance.LogException(ex);
			}
			LseLogger.Instance.LogDebug(string.Format("END - Processing [Client] {0}", GetType().Name));
		}

		private void InvokeServerProcessing()
		{
			LseLogger.Instance.LogDebug(string.Format("START - Processing [Server] {0}", GetType().Name));

			try
			{
				ProcessServer();
			}
			catch (Exception ex)
			{
				LseLogger.Instance.LogException(ex);
			}

			LseLogger.Instance.LogDebug(string.Format("END - Processing [Server] {0}", GetType().Name));
		}

		public abstract void ProcessClient();
		public abstract void ProcessServer();
	}
}