using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace SandboxGame.UI;

public partial class Notifications : Panel
{
	private static Notifications _instance;

	public Notifications()
	{
		StyleSheet.Load( "/ui/notifications/Notifications.scss" );
		_instance = this;
	}

	private void AddNotification( Notification notification )
	{
		Host.AssertClient();
		AddChild( notification );
	}

	private void AddNotification( string type, string text, float lifetime = 5f )
	{
		Host.AssertClient();

		text = Language.TryGetPhrase( text );
		AddNotification( new Notification( text, lifetime, type ) );
	}

	private void AddNotification<T>( string type, string text, T arg1, float lifetime = 5f )
	{
		Host.AssertClient();

		text = Language.TryGetPhrase( text, arg1 );
		AddNotification( type, text, lifetime );
	}

	private void AddNotification<T1, T2>( string type, string text, T1 arg1, T2 arg2, float lifetime = 5f )
	{
		Host.AssertClient();

		text = Language.TryGetPhrase( text, arg1, arg2 );
		AddNotification( type, text, lifetime );
	}

	private bool EditNotification( string type, string text, float lifetime = 5f )
	{
		Host.AssertClient();
		var notification = GetNotification( type );
		if ( notification is null )
		{
			return false;
		}

		text = Language.TryGetPhrase( text );
		notification.Text = text;
		notification.Lifetime = -lifetime;
		notification.Jiggle = -0.1f;

		return true;
	}

	private bool EditNotification<T>( string type, string text, T arg1, float lifetime = 5f )
	{
		Host.AssertClient();

		text = Language.TryGetPhrase( text, arg1 );
		return EditNotification( type, text, lifetime );
	}

	private bool EditNotification<T1, T2>( string type, string text, T1 arg1, T2 arg2, float lifetime = 5f )
	{
		Host.AssertClient();

		text = Language.TryGetPhrase( text, arg1, arg2 );
		return EditNotification( type, text, lifetime );
	}

	private Notification GetNotification( string type )
	{
		Host.AssertClient();
		return (Notification)Children.FirstOrDefault( notif =>
			((Notification)notif).Type == type && !notif.IsDeleting );
	}

	[ClientRpc]
	public static void Send( string text, float lifetime = 5f )
	{
		_instance.AddNotification( "generic", text, lifetime );
	}

	[ClientRpc]
	public static void Send( string type, string text, float lifetime = 5f )
	{
		if ( _instance.EditNotification( type, text, lifetime ) )
		{
			return;
		}

		_instance.AddNotification( type, text, lifetime );
	}

	[ClientRpc]
	public static void Send( string type, string text, string arg1, float lifetime = 5f )
	{
		if ( _instance.EditNotification( type, text, arg1, lifetime ) )
		{
			return;
		}

		_instance.AddNotification( type, text, arg1, lifetime );
	}

	[ClientRpc]
	public static void Send( string type, string text, string arg1, string arg2, float lifetime = 5f )
	{
		if ( _instance.EditNotification( type, text, arg1, arg2, lifetime ) )
		{
			return;
		}

		_instance.AddNotification( type, text, arg1, arg2, lifetime );
	}
}
