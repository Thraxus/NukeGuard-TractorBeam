using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.ModAPI;

namespace NukeGuard_TractorBeam.TractorBeams.Utilities
{
	public static class PlayerExtensions
	{
		// This should only be called on the client where it will have an effect, so we can cache it for now
		// TODO: Refresh this cache on a promotion event
		static readonly Dictionary<ulong, bool> _cachedResult = new Dictionary<ulong, bool>();


		/// <summary>
		/// Determines if the player is an Administrator of the active game session.
		/// </summary>
		/// <param name="player"></param>
		/// <returns>True if is specified player is an Administrator in the active game.</returns>
		public static bool IsAdmin(this IMyPlayer player)
		{
			// Offline mode. You are the only player.
			if (Sandbox.ModAPI.MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE)
			{
				return true;
			}

			// Hosted game, and the player is hosting the server.
			if (player.IsHost())
			{
				return true;
			}

			if (!_cachedResult.ContainsKey(player.SteamUserId))
			{
				// determine if client is admin of Dedicated server.
				List<MyObjectBuilder_Client> clients = Sandbox.ModAPI.MyAPIGateway.Session.GetCheckpoint("null").Clients;
				if (clients != null)
				{
					MyObjectBuilder_Client client = clients.FirstOrDefault(c => c.SteamId == player.SteamUserId && c.IsAdmin);
					_cachedResult[player.SteamUserId] = (client != null);
					return _cachedResult[player.SteamUserId];
					// If user is not in the list, automatically assume they are not an Admin.
				}

				// clients is null when it's not a dedicated server.
				// Otherwise Treat everyone as Normal Player.
				_cachedResult[player.SteamUserId] = false;
				return false;
			}
			else
			{
				//Logger.Instance.LogMessage("Using cached value");
				if( LseLogger.Instance.Debug )
					Sandbox.ModAPI.MyAPIGateway.Utilities.ShowNotification("Used cached admin check.", 100);
				return _cachedResult[player.SteamUserId];
			}
		}

		public static bool IsHost(this IMyPlayer player)
		{
			return Sandbox.ModAPI.MyAPIGateway.Multiplayer.IsServerPlayer(player.Client);
		}
	}
}