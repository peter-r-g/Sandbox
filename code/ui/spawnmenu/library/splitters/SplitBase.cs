using System;
using Sandbox.UI;

namespace SandboxGame.UI;

public class SplitButton : Button
{
	private readonly SplitBase Host;
	private Panel CategoryPanel;
	private Action ClickCallback;
	public bool IsButton;

	private Func<Panel> PanelCreator;

	public SplitButton( SplitBase host, string buttonName ) : base( buttonName )
	{
		Host = host;
		BindClass( "current", () => Host.CurrentButton == this );
	}

	public SplitButton WithCallback( Action clickCallback, bool isButton = false )
	{
		ClickCallback = clickCallback;
		IsButton = isButton;
		return this;
	}

	public SplitButton WithPanel( Func<Panel> panelCreator )
	{
		PanelCreator = panelCreator;
		return this;
	}

	public SplitButton WithPanel( Panel panel )
	{
		PanelCreator = () => panel;
		return this;
	}

	public SplitButton SetPanel( Panel panel )
	{
		if ( CategoryPanel != null )
		{
			CategoryPanel.Delete();
		}

		if ( panel == null )
		{
			return this;
		}

		CategoryPanel = panel;
		Host.CategoryContent.AddChild( CategoryPanel );
		CategoryPanel.Style.Display = DisplayMode.None;
		if ( Host.CurrentButton == this )
		{
			SetActive();
		}

		return this;
	}

	public SplitButton SetActive()
	{
		Host.SetActiveButton( this );
		return this;
	}

	public Panel GetPanel()
	{
		if ( PanelCreator != null )
		{
			if ( CategoryPanel == null )
			{
				CategoryPanel = PanelCreator.Invoke();
				Host.CategoryContent.AddChild( CategoryPanel );
			}

			return CategoryPanel;
		}

		return CategoryPanel;
	}

	protected override void OnClick( MousePanelEvent e )
	{
		Host.SetActiveButton( this );
		ClickCallback?.Invoke();
		base.OnClick( e );
	}
}

[UseTemplate]
public class SplitBase : Panel
{
	public SplitButton CurrentButton { get; set; }
	public Panel CategoryContent { get; set; }
	public Panel CategoryList { get; set; }

	public SplitButton Register( string categoryName )
	{
		SplitButton spButton = new(this, categoryName);
		CategoryList.AddChild( spButton );
		return spButton;
	}

	public void SetActiveButton( SplitButton splitButton )
	{
		if ( splitButton.IsButton )
		{
			return;
		}

		if ( CurrentButton is not null && CurrentButton.GetPanel() is Panel bPnl )
		{
			bPnl.Style.Display = DisplayMode.None;
		}

		CurrentButton = splitButton;
		if ( CurrentButton.GetPanel() is Panel pnl )
		{
			pnl.Style.Display = DisplayMode.Flex;
		}
	}
}
