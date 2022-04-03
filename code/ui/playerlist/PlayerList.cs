using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace SandboxGame.UI;

[UseTemplate]
public class PlayerList : Panel
{
	private readonly Dictionary<EntryCategory, Panel> CatePanels = new();
	private readonly List<PlayerEntry> Entries = new();
	public Panel Body { get; set; }

	public Label TitleLabel { get; set; }
	public Label KillsLabel { get; set; }
	public Label DeathsLabel { get; set; }
	public Label EntitiesLabel { get; set; }

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();

		TitleLabel.Text = Language.TryGetPhrase( TitleLabel.Text );
		KillsLabel.Text = Language.TryGetPhrase( KillsLabel.Text );
		DeathsLabel.Text = Language.TryGetPhrase( DeathsLabel.Text );
		EntitiesLabel.Text = Language.TryGetPhrase( EntitiesLabel.Text );
	}

	private Panel GetCategory( EntryCategory category )
	{
		if ( CatePanels.ContainsKey( category ) )
		{
			return CatePanels[category];
		}

		var catePanel = Body.Add.Panel( "category" );
		var header = catePanel.Add.Panel( "catheader" );
		header.Add.Label( category.Name, "cattitle" );
		CatePanels[category] = catePanel;

		Body.SortChildren( pnl =>
		{
			if ( !CatePanels.ContainsValue( pnl ) )
			{
				return -1;
			}

			return CatePanels.FirstOrDefault( x => x.Value == pnl ).Key.Order;
		} );

		return catePanel;
	}


	public override void Tick()
	{
		base.Tick();

		SetClass( "open", Input.Down( InputButton.Score ) );
		if ( !IsVisible )
		{
			return;
		}

		var players = Entries.Select( x => x.Owner );
		foreach ( var cl in Client.All.Except( players ) )
		{
			Entries.Add( new PlayerEntry( cl ) );
		}

		foreach ( var entry in Entries.ToList() )
		{
			var category = entry.GetCategory();

			if ( category == null )
			{
				Entries.Remove( entry );
				entry.Delete();
				continue;
			}

			if ( entry.Parent != GetCategory( category ) )
			{
				entry.Parent = GetCategory( category );
			}
		}

		foreach ( var (key, value) in CatePanels.ToList() )
		{
			if ( value.ChildrenCount > 1 )
			{
				continue;
			}

			CatePanels.Remove( key );
			value.Delete();
		}
	}
}
