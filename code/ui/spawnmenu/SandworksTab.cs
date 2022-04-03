using System;
using System.Threading;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Tests;

namespace SandboxGame.UI;

[UseTemplate]
public class SandworksTab : Panel
{
	private string filterText;
	private Package.Order order = Package.Order.Newest;

	private CancellationTokenSource PopulateCancelSource;
	private int skip;
	private int take = 1000;
	public VirtualScrollPanel ItemsContainer { get; set; }
	public FilterTextEntry FilterTextEntry { get; set; }
	public SpawnMenu SpawnMenu { get; set; }

	public Button NewestButton { get; set; }
	public Button PopularButton { get; set; }
	public Button TrendingButton { get; set; }
	public Button UpdatedButton { get; set; }
	public Button RefreshButton { get; set; }

	private Package.Order Order
	{
		get => order;
		set
		{
			order = value;
			Populate();
		}
	}

	private int Take
	{
		get => take;
		set
		{
			take = value;
			Populate();
		}
	}

	private int Skip
	{
		get => skip;
		set
		{
			skip = value;
			Populate();
		}
	}

	private string FilterText
	{
		get => filterText;
		set
		{
			filterText = value;
			Populate();
		}
	}

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();

		FilterTextEntry.OnValueChangedFunc = value =>
		{
			if ( string.IsNullOrWhiteSpace( value ) )
			{
				FilterText = null;
				return;
			}

			FilterText = value;
		};

		FilterTextEntry.Placeholder = Language.TryGetPhrase( FilterTextEntry.Placeholder );
		NewestButton.Text = Language.TryGetPhrase( NewestButton.Text );
		PopularButton.Text = Language.TryGetPhrase( PopularButton.Text );
		TrendingButton.Text = Language.TryGetPhrase( TrendingButton.Text );
		UpdatedButton.Text = Language.TryGetPhrase( UpdatedButton.Text );
		RefreshButton.Text = Language.TryGetPhrase( RefreshButton.Text );

		ItemsContainer.Layout.AutoColumns = true;
		ItemsContainer.Layout.ItemHeight = 150;
		ItemsContainer.Layout.ItemWidth = 150;
		ItemsContainer.AddClass( "canvas" );

		ItemsContainer.OnCreateCell = ( cell, data ) =>
		{
			var file = (Package)data;
			var spIcon = new SpawnIcon( file.Title )
				.WithIcon( file.Thumb )
				.WithCallback( isLeftClick =>
				{
					if ( isLeftClick )
					{
						_ = SandboxGame.SpawnModelCmd( file.FullIdent );
						return;
					}

					PopupMenu menu = new(SpawnMenu);
					var contextActions = Library.GetAttributes<PropContextAction>();
					foreach ( var contextAction in contextActions )
					{
						menu.AddButton( Language.TryGetPhrase( contextAction.ActionName ),
							() => contextAction.InvokeStatic( file.FullIdent, true ) );
					}
				} );

			cell.AddChild( spIcon );
		};

		Populate();
	}

	public void ButtonClick( string packageOrder )
	{
		Order = Enum.Parse<Package.Order>( packageOrder );
	}

	public void Populate()
	{
		PopulateCancelSource?.Cancel();
		PopulateCancelSource = new CancellationTokenSource();
		_ = PopulateTab( PopulateCancelSource.Token );
	}

	private async Task PopulateTab( CancellationToken ct )
	{
		if ( ct.IsCancellationRequested )
		{
			return;
		}

		var q = new Package.Query
		{
			Type = Package.Type.Model,
			Order = Order,
			Take = Take,
			Skip = Skip,
			SearchText = FilterText
		};
		var found = await q.RunAsync( ct );

		if ( ct.IsCancellationRequested )
		{
			return;
		}

		ItemsContainer.SetItems( found );
	}
}
