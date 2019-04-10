using System;
using System.Diagnostics;

namespace NukeGuard_TractorBeam.TractorBeams.Utilities
{
	/// <inheritdoc cref="Profiler" />
	/// <summary>
	/// This code was provided by Digi as a simple profiler
	/// Usage:
	///		Wrap code you want to profile in:
	///			using(new Profiler("somename"))
	///			{
	///				// code to profile
	///			}
	/// </summary>
	public struct Profiler : IDisposable
	{
		private readonly string _name;
		private readonly long _start;
		private readonly Log _profilingLog;

		public Profiler(string name = "profiler")
		{
			_profilingLog = new Log(name);
			_name = name;
			_start = Stopwatch.GetTimestamp();
		}

		public void Dispose()
		{
			long end = Stopwatch.GetTimestamp();
			TimeSpan timespan = new TimeSpan(end - _start);
			if(Settings.EnableProfilingLog)
				_profilingLog?.WriteToLog(_name, $"{timespan.TotalMilliseconds:0.##########}ms");
		}
	}
}
