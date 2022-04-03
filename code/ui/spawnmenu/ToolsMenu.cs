using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using SandboxGame.Entities;
using SandboxGame.Tools;
using SandboxGame.Weapons;

namespace SandboxGame.UI;

public class ToolsMenu : TabSplit
{
	private readonly Dictionary<string, SplitButton> ToolButtons = new();
	private BaseTool LastTool;

	public ToolsMenu()
	{
		StyleSheet.Load( "/ui/spawnmenu/ToolsMenu.scss" );
		ToolList();
	}

	private void ToolList()
	{
		var tabSplit = AddChild<TabSplit>();

		var tabMethods = Library.GetAttributes<ToolsMenuTab>();
		SplitButton first = null;
		foreach ( var tabMethod in tabMethods )
		{
			var splitButton = tabSplit.Register( Language.TryGetPhrase( tabMethod.TabName ) )
				.WithPanel( (Panel)tabMethod.InvokeStatic( this, tabSplit ) );
			first ??= splitButton;
		}

		first?.SetActive();
	}

	private void DisplayCurrent( SplitButton tBtn )
	{
		if ( Local.Pawn is null )
		{
			return;
		}

		var userTool = (ToolGun)Local.Pawn.Children.FirstOrDefault( x => x is ToolGun );

		if ( userTool != null && userTool.CurrentTool != null )
		{
			var sPanel = userTool.CurrentTool.MakeSettingsPanel();
			tBtn.SetPanel( sPanel ).SetActive();
		}
	}

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is null )
		{
			return;
		}

		var userTool = (ToolGun)Local.Pawn.Children.FirstOrDefault( x => x is ToolGun );
		var lastToolName = LastTool is { } tool ? tool.ClassInfo.Name : "";

		if ( userTool?.CurrentTool is not { } currentTool || currentTool.ClassInfo.Name == lastToolName )
		{
			return;
		}

		var entry = Library.GetAttribute( currentTool.GetType() );
		LastTool = currentTool;

		if ( !ToolButtons.TryGetValue( entry.Name, out var tBtn ) )
		{
			return;
		}

		var sPanel = currentTool.MakeSettingsPanel();
		tBtn.SetPanel( sPanel ).SetActive();
	}

	[ToolsMenuTab( TabName = "#tab_tools" )]
	public static Panel ToolsTab( ToolsMenu toolsMenu, TabSplit tabSplit )
	{
		CategorySplit catSplit = new();

		toolsMenu.BindClass( "small", () => catSplit.CurrentButton?.GetPanel() is null );

		foreach ( var entry in Library.GetAllAttributes<BaseTool>() )
		{
			if ( entry.Title == "BaseTool" || entry is LibraryMethod )
			{
				continue;
			}

			var titleText = entry.Title;
			titleText = Language.TryGetPhrase( titleText );

			var toolButton = catSplit.Register( titleText )
				.WithCallback( () => SandboxPlayer.EquipToolgunWithTool( entry.Name ) );

			toolsMenu.ToolButtons[entry.Name] = toolButton;
			if ( entry.Name == ConsoleSystem.GetValue( "tool_current" ) )
			{
				toolsMenu.DisplayCurrent( toolButton );
			}
		}

		return catSplit;
	}

	[ToolsMenuTab( TabName = "#tab_utils" )]
	public static Panel UtilsTab( ToolsMenu toolsMenu, TabSplit tabSplit )
	{
		CategorySplit catSplit = new();

		catSplit.Register( Language.TryGetPhrase( "#utilaction_undo" ) )
			.WithCallback( () => UndoHandler.UndoCmd(), true );

		catSplit.Register( Language.TryGetPhrase( "#utilaction_undoall" ) )
			.WithCallback( () => UndoHandler.UndoCmd( -1 ), true );

		catSplit.Register( Language.TryGetPhrase( "#utilaction_undoeveryone" ) )
			.WithCallback( UndoHandler.UndoEveryoneCmd, true );

		catSplit.Register( Language.TryGetPhrase( "#utilaction_cleanupmap" ) )
			.WithCallback( PlayerEntry.CleanupMap, true );

		return catSplit;
	}
}

public class ToolsMenuTab : LibraryMethod
{
	public string TabName { get; set; }
}
