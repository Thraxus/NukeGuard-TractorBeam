using Sandbox.ModAPI;

namespace NukeGuard_TractorBeam.TractorBeams
{
	public static class Settings
	{
		public static bool IsServer => MyAPIGateway.Multiplayer.IsServer;

		public static bool DebugMode { get; } = false;

		public static ushort NewtworkId => 25648;

		public const int TicksPerSecond = 60;

		public const int TicksPerMinute = TicksPerSecond * 60;

		public const int DefaultLocalMessageDisplayTime = 5000;

		public const int DefaultServerMessageDisplayTime = 10000;

		public static bool EnableGeneralLog { get; } = true;

		public static bool EnableProfilingLog { get; } = false;
	}
}
