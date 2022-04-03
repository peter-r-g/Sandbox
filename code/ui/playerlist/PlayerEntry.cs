using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace SandboxGame.UI;

public class EntryCategory
{
	public static EntryCategory Host = new("#scorecategory_host");
	public static EntryCategory Player = new("#scorecategory_player", 1);
	public static EntryCategory Bot = new("#scorecategory_bot", 2);
	public static EntryCategory Offline = new("#scorecategory_recentlyleft", 3);

	public string Name;
	public int Order;

	public EntryCategory( string name, int order = 0 )
	{
		Name = Language.TryGetPhrase( name );
		Order = order;
	}
}

[UseTemplate]
public class PlayerEntry : Panel
{
	public readonly Client Owner;
	private RealTimeSince LastUpdate = 0;
	private TimeSince OfflineTime;

	private long UserId;

	public PlayerEntry( Client cl )
	{
		UserId = cl.PlayerId;
		Owner = cl;
	}

	public string Entities { get; set; } = "0";
	public string Username { get; set; }
	public string Deaths { get; set; }
	public string Kills { get; set; }

	protected override void OnClick( MousePanelEvent e )
	{
		PopupMenu popup = new(this);
		var contextActions = Library.GetAttributes<PlayerContextAction>();
		foreach ( var contextAction in contextActions )
		{
			if ( Owner == Local.Client && !contextAction.TargetSelf )
			{
				continue;
			}

			if ( Owner != Local.Client && !contextAction.TargetOthers )
			{
				continue;
			}

			if ( !Local.Client.IsListenServerHost && contextAction.HostOnly )
			{
				continue;
			}

			popup.AddButton( Language.TryGetPhrase( contextAction.ActionName ),
				() => contextAction.InvokeStatic( Owner ) );
		}
	}

	protected override void OnRightClick( MousePanelEvent e )
	{
		OnClick( e );
	}

	public EntryCategory GetCategory()
	{
		if ( !Owner.IsValid() )
		{
			if ( OfflineTime <= 60 || Entities != "0" )
			{
				return EntryCategory.Offline;
			}

			return null;
		}

		OfflineTime = 0;

		if ( Owner.IsListenServerHost )
		{
			return EntryCategory.Host;
		}

		return Owner.IsBot ? EntryCategory.Bot : EntryCategory.Player;
	}

	public override void Tick()
	{
		base.Tick();

		if ( !IsVisible || LastUpdate < 0.1f )
		{
			return;
		}

		LastUpdate = 0;

		if ( UserId != 0 )
		{
			Entities = SandboxGame.Instance.UndoCount.TryGetValue( UserId, out var undoCount )
				? undoCount.ToString()
				: "0";
		}

		if ( !Owner.IsValid() )
		{
			Deaths = "-";
			Kills = "-";
			return;
		}

		Username = Owner.Name;
		UserId = Owner.PlayerId;
		Deaths = Owner.GetInt( "deaths" ).ToString();
		Kills = Owner.GetInt( "kills" ).ToString();
	}

	[PlayerContextAction( ActionName = "#playeraction_copyname", TargetSelf = true, TargetOthers = true,
		HostOnly = false )]
	public static void CopyName( Client cl )
	{
		Clipboard.SetText( cl.Name );
	}

	[PlayerContextAction( ActionName = "#playeraction_copysteamid", TargetSelf = true, TargetOthers = true,
		HostOnly = false )]
	public static void CopySteamId( Client cl )
	{
		Clipboard.SetText( cl.PlayerId.ToString() );
	}

	[PlayerContextAction( ActionName = "#playeraction_goto", TargetSelf = false, TargetOthers = true,
		HostOnly = false )]
	public static void GotoPlayer( Client cl )
	{
		GotoPlayerCmd( cl.PlayerId.ToString() );
	}

	[PlayerContextAction( ActionName = "#playeraction_undoall", TargetSelf = true, TargetOthers = true,
		HostOnly = true )]
	public static void CleanupPlayer( Client cl )
	{
		CleanupPlayerCmd( cl.PlayerId.ToString() );
	}

	[PlayerContextAction( ActionName = "#playeraction_kick", TargetSelf = false, TargetOthers = true, HostOnly = true )]
	public static void KickPlayer( Client cl )
	{
		KickPlayerCmd( cl.PlayerId.ToString() );
	}
	
	[ServerCmd]
	public static void CleanupMap()
	{
		if ( ConsoleSystem.Caller is null || ConsoleSystem.Caller.IsListenServerHost )
		{
			UndoHandler.UndoEveryoneCmd();
			Map.Reset( SandboxGame.CleanupFilter );
			Log.Info( Language.GetPhrase( "map_cleaned" ) );
			Notifications.Send( To.Single( ConsoleSystem.Caller ), "#map_cleaned" );
			return;
		}

		Notifications.Send( To.Single( ConsoleSystem.Caller ), "#server_host_only" );
	}

	[ServerCmd]
	public static void GotoPlayerCmd( string userIdStr )
	{
		if ( !long.TryParse( userIdStr, out var userId ) )
		{
			return;
		}

		var target = Client.All.FirstOrDefault( x => x.PlayerId == userId );

		if ( ConsoleSystem.Caller is not Client cl || target == null )
		{
			return;
		}

		Notifications.Send( To.Single( target ), Language.GetPhrase( "teleported_to_you", cl.Name ) );
		cl.Pawn.Position = target.Pawn.Position;
	}

	[ServerCmd]
	public static void CleanupPlayerCmd( string userIdStr )
	{
		if ( ConsoleSystem.Caller is not {IsListenServerHost: true} )
		{
			return;
		}

		if ( !long.TryParse( userIdStr, out var userId ) )
		{
			return;
		}

		UndoHandler.DoUndo( userId, -1 );
	}

	[ServerCmd]
	public static void KickPlayerCmd( string userIdStr )
	{
		if ( ConsoleSystem.Caller is not {IsListenServerHost: true} )
		{
			return;
		}

		if ( !long.TryParse( userIdStr, out var userId ) )
		{
			return;
		}

		var target = Client.All.FirstOrDefault( x => x.PlayerId == userId );
		target?.Kick();
	}
}

public class PlayerContextAction : LibraryMethod
{
	public string ActionName { get; set; }
	public bool TargetSelf { get; set; }
	public bool TargetOthers { get; set; }
	public bool HostOnly { get; set; }
}
