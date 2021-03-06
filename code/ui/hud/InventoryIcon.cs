using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace SandboxGame.UI;

public class InventoryIcon : Panel
{
	public Label Label;
	public Label Number;
	public Entity TargetEnt;

	public InventoryIcon( int i, Panel parent )
	{
		Parent = parent;
		Label = Add.Label( "empty", "item-name" );
		Number = Add.Label( $"{i}", "slot-number" );
	}

	public void Clear()
	{
		Label.Text = "";
		SetClass( "active", false );
	}
}
