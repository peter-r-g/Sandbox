using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace SandboxGame.UI;

[UseTemplate]
public class PopupMenu : Panel
{
	private readonly Panel Caller;


	public PopupMenu( Panel caller )
	{
		Parent = caller.FindRootPanel();
		PopupInner.Style.Left = Mouse.Position.x;
		PopupInner.Style.Top = Mouse.Position.y;
		Caller = caller;
	}

	public Panel PopupInner { get; set; }

	public void AddButton( string text, Action callback )
	{
		PopupInner.Add.Button( text, () =>
		{
			callback();
			Delete();
		} );
	}

	protected override void OnClick( MousePanelEvent e )
	{
		base.OnClick( e );

		if ( !PopupInner.IsInside( Mouse.Position ) )
		{
			Delete();

			Caller.FindRootPanel().CreateEvent( "onmousedown" );
		}
	}

	public override void Tick()
	{
		base.Tick();

		if ( !Caller.IsVisible )
		{
			Delete();
		}
	}
}
