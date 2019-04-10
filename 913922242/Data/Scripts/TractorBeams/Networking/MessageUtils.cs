using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NukeGuard_TractorBeam.TractorBeams.Utilities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace NukeGuard_TractorBeam.TractorBeams.Networking
{
	public static class MessageUtils
	{
		public static List<byte> ClientMessageCache = new List<byte>();
		public static Dictionary<ulong, List<byte>> ServerMessageCache = new Dictionary<ulong, List<byte>>();

		public static readonly ushort MessageId = 5402;
		static readonly int MaxMessageSize = 4096;

		public static void SendMessageToServer(MessageBase message)
		{
			message.Side = MessageSide.ServerSide;
			if (MyAPIGateway.Session.Player != null)
				message.SenderSteamId = MyAPIGateway.Session.Player.SteamUserId;
			string xml = MyAPIGateway.Utilities.SerializeToXML<MessageContainer>(new MessageContainer() { Content = message });
			byte[] byteData = Encoding.UTF8.GetBytes(xml);
			LseLogger.Instance.LogDebug(string.Format("SendMessageToServer {0} {1} {2}, {3}b", message.SenderSteamId, message.Side, message.GetType().Name, byteData.Length));
			if (byteData.Length <= MaxMessageSize)
				MyAPIGateway.Multiplayer.SendMessageToServer(MessageId, byteData);
			else
				SendMessageParts(byteData, MessageSide.ServerSide);
		}

		/// <summary>
		/// Creates and sends an entity with the given information for the server and all players.
		/// </summary>
		/// <param name="content"></param>
		public static void SendMessageToAll(MessageBase message, bool syncAll = true)
		{
			if (MyAPIGateway.Session.Player != null)
				message.SenderSteamId = MyAPIGateway.Session.Player.SteamUserId;

			if (syncAll || !MyAPIGateway.Multiplayer.IsServer)
			{
				SendMessageToServer(message);
			}
			SendMessageToAllPlayers(message);
		}

		public static void SendMessageToAllPlayers(MessageBase messageContainer)
		{
			//MyAPIGateway.Multiplayer.SendMessageToOthers(StandardClientId, System.Text.Encoding.Unicode.GetBytes(ConvertData(content))); <- does not work as expected ... so it doesn't work at all?
			List<IMyPlayer> players = new List<IMyPlayer>();
			MyAPIGateway.Players.GetPlayers(players, p => p != null && !p.IsHost());
			foreach (IMyPlayer player in players)
			{
				SendMessageToPlayer(player.SteamUserId, messageContainer);
			}
		}

		public static void SendMessageToPlayer(ulong steamId, MessageBase message)
		{
			message.Side = MessageSide.ClientSide;
			string xml = MyAPIGateway.Utilities.SerializeToXML(new MessageContainer() { Content = message });
			byte[] byteData = Encoding.UTF8.GetBytes(xml);

			LseLogger.Instance.LogDebug(string.Format("SendMessageToPlayer {0} {1} {2}, {3}b", steamId, message.Side, message.GetType().Name, byteData.Length));
            
			if (byteData.Length <= MaxMessageSize)
				MyAPIGateway.Multiplayer.SendMessageTo(MessageId, byteData, steamId);
			else
				SendMessageParts(byteData, MessageSide.ClientSide, steamId);
		}
			
		#region Message Splitting
		/// <summary>
		/// Calculates how many bytes can be stored in the given message.
		/// </summary>
		/// <param name="message">The message in which the bytes will be stored.</param>
		/// <returns>The number of bytes that can be stored until the message is too big to be sent.</returns>
		public static int GetFreeByteElementCount(MessageIncomingMessageParts message)
		{
			message.Content = new byte[1];
			string xmlText = MyAPIGateway.Utilities.SerializeToXML<MessageContainer>(new MessageContainer() { Content = message });
			int oneEntry = Encoding.UTF8.GetBytes(xmlText).Length;

			message.Content = new byte[4];
			xmlText = MyAPIGateway.Utilities.SerializeToXML<MessageContainer>(new MessageContainer() { Content = message });
			int twoEntries = Encoding.UTF8.GetBytes(xmlText).Length;

			// we calculate the difference between one and two entries in the array to get the count of bytes that describe one entry
			// we divide by 3 because 3 entries are stored in one block of the array
			double difference = (double)(twoEntries - oneEntry) / 3d;

			// get the size of the message without any entries
			double freeBytes = MaxMessageSize - oneEntry - Math.Ceiling(difference);

			int count = (int)Math.Floor((double)freeBytes / difference);

			// finally we test if the calculation was right
			message.Content = new byte[count];
			xmlText = MyAPIGateway.Utilities.SerializeToXML<MessageContainer>(new MessageContainer() { Content = message });
			int finalLength = Encoding.UTF8.GetBytes(xmlText).Length;
			LseLogger.Instance.LogDebug(string.Format("FinalLength: {0}", finalLength));
			if (MaxMessageSize >= finalLength)
				return count;
			else
				throw new Exception(string.Format("Calculation failed. OneEntry: {0}, TwoEntries: {1}, Difference: {2}, FreeBytes: {3}, Count: {4}, FinalLength: {5}", oneEntry, twoEntries, difference, freeBytes, count, finalLength));
		}

		private static void SendMessageParts(byte[] byteData, MessageSide side, ulong receiver = 0)
		{
			LseLogger.Instance.LogDebug(string.Format("SendMessageParts {0} {1} {2}", byteData.Length, side, receiver));

			List<byte> byteList = byteData.ToList();

			while (byteList.Count > 0)
			{
				// we create an empty message part
				MessageIncomingMessageParts messagePart = new MessageIncomingMessageParts()
				{
					Side = side,
					SenderSteamId = side == MessageSide.ServerSide ? MyAPIGateway.Session.Player.SteamUserId : 0,
					LastPart = false,
				};

				try
				{
					// let's check how much we could store in the message
					int freeBytes = GetFreeByteElementCount(messagePart);

					int count = freeBytes;

					// we check if that might be the last message
					if (freeBytes > byteList.Count)
					{
						messagePart.LastPart = true;

						// since we changed LastPart, we should make sure that we are still able to send all the stuff
						if (GetFreeByteElementCount(messagePart) > byteList.Count)
						{
							count = byteList.Count;
						}
						else
							throw new Exception("Failed to send message parts. The leftover could not be sent!");
					}

					// fill the message with content
					messagePart.Content = byteList.GetRange(0, count).ToArray();
					string xmlPart = MyAPIGateway.Utilities.SerializeToXML<MessageContainer>(new MessageContainer() { Content = messagePart });
					byte[] bytes = Encoding.UTF8.GetBytes(xmlPart);

					// and finally send the message
					switch (side)
					{
						case MessageSide.ClientSide:
							if (MyAPIGateway.Multiplayer.SendMessageTo(MessageId, bytes, receiver))
								byteList.RemoveRange(0, count);
							else
								throw new Exception("Failed to send message parts to client.");
							break;
						case MessageSide.ServerSide:
							if (MyAPIGateway.Multiplayer.SendMessageToServer(MessageId, bytes))
								byteList.RemoveRange(0, count);
							else
								throw new Exception("Failed to send message parts to server.");
							break;
					}

				}
				catch (Exception ex)
				{
					LseLogger.Instance.LogException(ex);
					return;
				}
			}
		}
		#endregion

		public static void HandleMessage(byte[] rawData)
		{
			try
			{
				string data = Encoding.UTF8.GetString(rawData);
				MessageContainer message = MyAPIGateway.Utilities.SerializeFromXML<MessageContainer>(data);

				LseLogger.Instance.LogDebug("HandleMessage()");
				if (message != null && message.Content != null)
				{
					message.Content.InvokeProcessing();
				}
				return;
			}
			catch (Exception e)
			{
				// Don't warn the user of an exception, this can happen if two mods with the same message id receive an unknown message
				LseLogger.Instance.LogMessage(string.Format("Processing message exception. Exception: {0}", e));
				//Logger.Instance.LogException(e);
			}

		}
	}
}