using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using SandboxGame.Tools;
using SandboxGame.Weapons;

namespace SandboxGame.UI;

public class CurrentTool : Panel
{
	public Label Description;
	public Label Title;

	public CurrentTool()
	{
		Title = Add.Label( "", "title" );
		Description = Add.Label( "", "description" );
	}

	public override void Tick()
	{
		var tool = GetCurrentTool();
		SetClass( "active", tool != null );

		if ( tool is null )
		{
			return;
		}

		var toolClass = tool.ClassInfo.Name;
		Title.SetText( Language.TryGetPhrase( toolClass ) );
		Description.SetText( Language.TryGetPhrase( $"{toolClass}_description" ) );
	}

	private static BaseTool GetCurrentTool()
	{
		if ( Local.Pawn is not Player player )
		{
			return null;
		}

		var inventory = player.Inventory;
		return inventory?.Active is not ToolGun tool ? null : tool.CurrentTool;
	}
}
