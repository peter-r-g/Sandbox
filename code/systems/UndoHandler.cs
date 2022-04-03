using System;
using System.Collections.Generic;
using Sandbox;
using SandboxGame.UI;

namespace SandboxGame
{
	public struct UndoEntry
	{
		public Func<bool> Undo;
		public Entity Prop;
	}

	public static class UndoHandler
	{
		private static readonly Dictionary<long, List<UndoEntry>> Props = new();

		public static int DoUndo( long userId, int count = 1 )
		{
			var userProps = Props.GetValueOrDefault( userId );
			if ( userProps == null )
			{
				return 0;
			}

			var total = 0;
			if ( count == -1 )
			{
				count = int.MaxValue;
			}

			while ( total < count && userProps.Count > 0 )
			{
				var entry = userProps[^1];
				userProps.Remove( entry );

				if ( AttemptUndo( entry ) )
				{
					total++;
				}
			}

			SandboxGame.Instance.UndoCount[userId] = userProps.Count;
			return total;
		}

		private static bool AttemptUndo( UndoEntry entry )
		{
			if ( entry.Prop is null )
			{
				return entry.Undo != null && entry.Undo();
			}

			if ( !entry.Prop.IsValid )
			{
				return false;
			}

			entry.Prop.Delete();
			return true;
		}

		private static List<UndoEntry> GetPlayerEntry( Client client )
		{
			if ( !Props.ContainsKey( client.PlayerId ) )
			{
				Props[client.PlayerId] = new List<UndoEntry>();
			}

			return Props[client.PlayerId];
		}

		public static void Register( Entity player, Entity prop )
		{
			var propList = GetPlayerEntry( player.Client );
			propList.Add( new UndoEntry {Prop = prop} );

			SandboxGame.Instance.UndoCount[player.Client.PlayerId] = propList.Count;
		}

		public static void Register( Entity player, Func<bool> func )
		{
			var propList = GetPlayerEntry( player.Client );
			propList.Add( new UndoEntry {Undo = func} );

			SandboxGame.Instance.UndoCount[player.Client.PlayerId] = propList.Count;
		}

		[ServerCmd]
		public static void UndoCmd( int amnt = 1 )
		{
			var cl = ConsoleSystem.Caller;
			if ( cl == null )
			{
				Log.Warning( Language.GetPhrase( "console_cant_use" ) );
				return;
			}

			var undone = DoUndo( cl.PlayerId, amnt );
			if ( undone == 0 )
			{
				return;
			}

			Notifications.SendUndo( To.Single( cl ), undone );
		}

		[ServerCmd]
		public static void UndoEveryoneCmd()
		{
			if ( ConsoleSystem.Caller is null || ConsoleSystem.Caller.IsListenServerHost )
			{
				foreach ( var id in Props.Keys )
				{
					DoUndo( id, -1 );
				}

				Log.Info( Language.GetPhrase( "all_undone" ) );
				Notifications.Send( To.Single( ConsoleSystem.Caller ), "#all_undone", 3 );
				return;
			}

			Notifications.Send( To.Single( ConsoleSystem.Caller ), "#server_host_only" );
		}
	}
}

namespace SandboxGame.UI
{
	public partial class Notifications
	{
		[ClientRpc]
		public static void HandleUndoRpc( int count )
		{
			var notif = _instance.GetNotification( "undo" );
			if ( notif is not null )
			{
				var newCount = (int)notif.Data + count;
				notif.Data = newCount;
				notif.Text = Language.GetPhrase( "undone", newCount, newCount > 1 ? "s" : "" );
				notif.Lifetime = -3;
				notif.Jiggle = -0.1f;
				return;
			}

			var notification = new Notification(
				Language.GetPhrase( "undone", count, count > 1 ? "s" : "" ),
				3,
				"undo",
				count
			);
			_instance.AddNotification( notification );
		}

		public static void SendUndo( To to, int count )
		{
			Host.AssertServer();
			HandleUndoRpc( to, count );
		}
	}
}
