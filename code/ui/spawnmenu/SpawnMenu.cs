using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Tests;

namespace SandboxGame.UI;

[UseTemplate]
public class SpawnMenu : Panel
{
	public SpawnMenu()
	{
		BindClass( "hidden", () => !(Input.Down( InputButton.Menu ) || HasFocus) );
		CreateSpawnMenu();
	}

	public Panel SpawnPanel { get; set; }
	public ToolsMenu ToolPanel { get; set; }

	private void CreateSpawnMenu()
	{
		var tabSplit = SpawnPanel.AddChild<TabSplit>();

		var tabMethods = Library.GetAttributes<SpawnMenuTab>();
		SplitButton first = null;
		foreach ( var tabMethod in tabMethods )
		{
			var title = Language.TryGetPhrase( tabMethod.TabName );
			var splitButton = tabSplit.Register( title ).WithPanel( (Panel)tabMethod.InvokeStatic( this, tabSplit ) );
			first ??= splitButton;
		}

		first?.SetActive();
	}

	private static VirtualScrollPanel MakeScrollPanel()
	{
		var canvas = new VirtualScrollPanel {Layout = {AutoColumns = true, ItemHeight = 150, ItemWidth = 150}};
		canvas.AddClass( "canvas" );
		return canvas;
	}

	private static string GetFileName( string name )
	{
		var fileMatch = Regex.Match( name, @"(\w+)\.\w+$" );
		return fileMatch.Groups[1].Value;
	}

	[SpawnMenuTab( TabName = "#tab_props" )]
	public static Panel PropsTab( SpawnMenu spawnMenu, TabSplit tabSplit )
	{
		var scrollPanel = MakeScrollPanel();

		scrollPanel.OnCreateCell = ( cell, data ) =>
		{
			var modelName = (string)data;
			var spIcon = new SpawnIcon( GetFileName( modelName ) );

			if ( Texture.Load( FileSystem.Mounted, $"/models/{modelName}_c.png", false ) is Texture tx )
			{
				spIcon.WithIcon( tx );
			}
			// Disabled as this is way too demanding
			/*else
				spIcon.WithRenderedIcon( $"models/{modelName}" );*/

			spIcon.WithCallback( isLeftClick =>
			{
				if ( isLeftClick )
				{
					_ = SandboxGame.SpawnModelCmd( $"models/{modelName}" );
					return;
				}

				PopupMenu menu = new(spawnMenu);
				var contextActions = Library.GetAttributes<PropContextAction>();
				foreach ( var contextAction in contextActions )
				{
					menu.AddButton( Language.TryGetPhrase( contextAction.ActionName ),
						() => contextAction.InvokeStatic( modelName, false ) );
				}
			} );

			cell.AddChild( spIcon );
		};

		var files = FileSystem.Mounted.FindFile( "models", "*.vmdl_c.png", true )
			.Concat( FileSystem.Mounted.FindFile( "models", "*.vmdl_c", true ) );

		List<string> alreadyAdded = new();
		foreach ( var file in files )
		{
			if ( string.IsNullOrWhiteSpace( file ) )
			{
				continue;
			}

			if ( file.Contains( "clothes" ) )
			{
				continue;
			}

			if ( file.Contains( "_lod0" ) )
			{
				continue;
			}

			if ( alreadyAdded.Contains( file ) )
			{
				continue;
			}

			var filePath = Regex.Match( file, @"^(.*\.vmdl)" ).Groups[1].Value;
			if ( alreadyAdded.Contains( filePath ) )
			{
				continue;
			}

			scrollPanel.AddItem( filePath );
			alreadyAdded.Add( filePath );
		}

		return scrollPanel;
	}

	[SpawnMenuTab( TabName = "#tab_entities" )]
	public static Panel EntitiesTab( SpawnMenu spawnMenu, TabSplit tabSplit )
	{
		var scrollPanel = MakeScrollPanel();

		scrollPanel.OnCreateCell = ( cell, data ) =>
		{
			var entry = (LibraryAttribute)data;
			var spIcon = new SpawnIcon( Language.TryGetPhrase( entry.Name ) )
				.WithIcon( $"/entity/{entry.Name}.png" )
				.WithCallback( isLeftClick =>
				{
					if ( isLeftClick )
					{
						SandboxGame.SpawnEntityCmd( entry.Name );
						return;
					}

					PopupMenu menu = new(spawnMenu);
					var contextActions = Library.GetAttributes<EntityContextAction>();
					foreach ( var contextAction in contextActions )
					{
						menu.AddButton( Language.TryGetPhrase( contextAction.ActionName ),
							() => contextAction.InvokeStatic( entry.Name ) );
					}
				} );

			cell.AddChild( spIcon );
		};

		var ents = Library.GetAllAttributes<Entity>().Where( x => x.Spawnable ).OrderBy( x => x.Title ).ToArray();
		foreach ( var entry in ents )
		{
			scrollPanel.AddItem( entry );
		}

		return scrollPanel;
	}

	[SpawnMenuTab( TabName = "#tab_sandworks" )]
	public static Panel SandworksTab( SpawnMenu spawnMenu, TabSplit tabSplit )
	{
		var tab = new SandworksTab {SpawnMenu = spawnMenu};
		return tab;
	}

	private static async Task PopulateSandworksTab( VirtualScrollPanel tab )
	{
		var q = new Package.Query {Type = Package.Type.Model, Order = Package.Order.Newest, Take = 1000};
		var found = await q.RunAsync( default );

		tab.SetItems( found );
	}

	[PropContextAction( ActionName = "#propaction_copypath" )]
	public static void CopyPathAction( string modelName, bool sandworks )
	{
		Clipboard.SetText( sandworks ? modelName : $"models/{modelName}" );
	}

	[EntityContextAction( ActionName = "#entityaction_copyclass" )]
	public static void CopyClassAction( string entityClass )
	{
		Clipboard.SetText( entityClass );
	}
}

public class SpawnMenuTab : LibraryMethod
{
	public string TabName { get; set; }
}

public class PropContextAction : LibraryMethod
{
	public string ActionName { get; set; }
}

public class EntityContextAction : LibraryMethod
{
	public string ActionName { get; set; }
}
