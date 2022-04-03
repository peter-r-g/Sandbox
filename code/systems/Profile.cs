#if DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using Sandbox;

namespace SandboxGame.Debug;

public static class Profile
{
	[SkipHotload]
	private static readonly ProfileData ProfiledData = new();
	private static readonly Stopwatch Sw = Stopwatch.StartNew();
	private static long NanosecondsPerTick => 1_000_000_000L / Stopwatch.Frequency;
	private static bool _profileActive;

	private static Entry _root = new();
	private static TimeSince _timeSince;

	public static IDisposable Scope( string name )
	{
		if ( !_profileActive )
			return null;
		
		return new ProfileScope( name );
	}
	
	[Event.Tick]
	public static void Frame()
	{
		if ( _timeSince >= 0.5f )
		{
			_timeSince = 0;

			DebugOverlay.ScreenText( 20, _root.GetString(), 0.5f );
		}

		_root.Wipe();
	}
	
	[Event.Hotload]
	public static void Hotloaded()
	{
		_root = new Entry();
	}

	[ServerCmd( Constants.Command.DebugTimerStats )]
	public static void DisplayTimerProperties()
	{
		// Display the timer frequency and resolution.
		Log.Info( Stopwatch.IsHighResolution
			? Language.TryGetPhrase( "profiling_highres" )
			: Language.TryGetPhrase( "profiling_lowres" ) );
		Log.Info( Language.TryGetPhrase( "profiling_accuracy", (1000L * 1000L * 1000L) / Stopwatch.Frequency ) );
	}

	[ClientCmd( Constants.Command.DebugSetActiveCl )]
	public static void SetActiveCl( bool active )
	{
		_profileActive = active;
		Log.Info( active
			? Language.TryGetPhrase( "profiling_active" )
			: Language.TryGetPhrase( "profiling_inactive" ) );
	}
	
	[ServerCmd( Constants.Command.DebugSetActiveSv )]
	public static void SetActiveSv( bool active )
	{
		_profileActive = active;
		Log.Info( active
			? Language.TryGetPhrase( "profiling_active" )
			: Language.TryGetPhrase( "profiling_inactive" ) );
	}
	
	[ClientCmd( Constants.Command.DebugSaveProfileCl )]
	public static void SaveProfileCl()
	{
		if ( ProfiledData.Profiles.Count == 0 )
		{
			Log.Warning( Language.TryGetPhrase( "profiling_nodata" ) );
			return;
		}
		
		FileSystem.Data.WriteJson( $"cl_{Constants.ProfileDataFileName}", ProfiledData );
		ProfiledData.Profiles.Clear();
		Log.Info( Language.TryGetPhrase( "profiling_saved", $"cl_{Constants.ProfileDataFileName}" ) );
	}

	[ServerCmd( Constants.Command.DebugSaveProfileSv )]
	public static void SaveProfileSv()
	{
		if ( ProfiledData.Profiles.Count == 0 )
		{
			Log.Warning( Language.TryGetPhrase( "profiling_nodata" ) );
			return;
		}
		
		FileSystem.Data.WriteJson( $"sv_{Constants.ProfileDataFileName}", ProfiledData );
		ProfiledData.Profiles.Clear();
		Log.Info( Language.TryGetPhrase( "profiling_saved", $"sv_{Constants.ProfileDataFileName}" ) );
	}

	[ClientCmd( Constants.Command.DebugDumpProfileCl )]
	public static void DumpProfileCl()
	{
		ProfiledData.Profiles.Clear();
		Log.Info( Language.TryGetPhrase( "profiling_cleared" ) );
	}
	
	[ServerCmd( Constants.Command.DebugDumpProfileSv )]
	public static void DumpProfileSv()
	{
		ProfiledData.Profiles.Clear();
		Log.Info( Language.TryGetPhrase( "profiling_cleared" ) );
	}
	
	private readonly struct ProfileScope : IDisposable
	{
		private readonly Entry Parent;
		private readonly Entry Me;
		
		private readonly string Name;
		private readonly long StartTicks;

		public ProfileScope( string name )
		{
			Parent = _root;
			Me = Parent.GetOrCreateChild( name );
			
			Name = name;
			StartTicks = Sw.ElapsedTicks;
			_root = Me;
		}

		public void Dispose()
		{
			var endTicks = Sw.ElapsedTicks;
			var startNano = StartTicks * NanosecondsPerTick;
			var endNano = endTicks * NanosecondsPerTick;
			
			ProfiledData.Profiles.Add( new ProfileEntry( Name, Host.IsServer ? "Server" : "Client",
				startNano / 1000, endNano / 1000 ) );

			Me.Add( (endNano - startNano) / (double)1000000 );
			_root = Parent;
		}
	}
	
	private class Entry
	{
		private string Name;
		private int Calls;
		private double Times;
		private List<Entry> Children;

		public Entry GetOrCreateChild( string name )
		{
			Children ??= new List<Entry>();

			foreach (var t in Children)
			{
				if ( t.Name == name )
					return t;
			}

			var e = new Entry {Name = name};

			Children.Add( e );
			return e;
		}

		public void Add( double v )
		{
			Calls++;
			Times += v;
		}

		public void Wipe()
		{
			Calls = 0;
			Times = 0;

			if ( Children == null ) return;

			foreach (var t in Children)
			{
				t.Wipe();
			}
		}

		public string GetString( int indent = 0 )
		{
			var str = $"{new string( ' ', indent*2)}{Times:0.00}ms  {Calls} - {Name}\n";

			if ( indent == 0 )
				str = "";

			if ( Children == null )
				return str;

			return Children.OrderByDescending( x => x.Times ).Where( child => child.Calls != 0 )
				.Aggregate( str, ( current, child ) => current + child.GetString( indent + 1 ) );
		}
	}

	private readonly struct ProfileData
	{
		[JsonPropertyName( "traceEvents" )] public List<ProfileEntry> Profiles { get; } = new();
		[JsonPropertyName( "displayTimeUnit" )] public string TimeUnit => "ns";
	}

	private readonly struct ProfileEntry
	{
		[JsonPropertyName( "name" )] public string Name { get; }
		[JsonPropertyName( "cat" )] public string Category { get; }
		[JsonPropertyName( "ts" )] public long StartTime { get; }
		[JsonPropertyName( "dur" )] public long Duration { get; }
		[JsonPropertyName( "pid" )] public int Pid => 0;
		[JsonPropertyName( "tid" )] public int Tid => 0;
		[JsonPropertyName( "ph" )] public string EventName => "X";

		public ProfileEntry( string name, string category, long startTime, long endTime )
		{
			Name = name;
			Category = category;
			StartTime = startTime;
			Duration = endTime - startTime;
		}
	}
}
#endif
