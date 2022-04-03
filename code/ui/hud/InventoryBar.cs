using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;
using SandboxGame.Weapons;

namespace SandboxGame.UI;

public class InventoryBar : Panel
{
	private readonly List<InventoryIcon> slots = new();

	private bool wasAlive;

	public InventoryBar()
	{
		for ( var i = 0; i < 9; i++ )
		{
			var icon = new InventoryIcon( i + 1, this );
			slots.Add( icon );
		}
	}

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not Player player )
		{
			return;
		}

		if ( player.Inventory is not Inventory inv )
		{
			return;
		}

		if ( !wasAlive && player.LifeState == LifeState.Alive )
		{
			inv.SortItems();
		}

		wasAlive = player.LifeState == LifeState.Alive;

		for ( var i = 0; i < slots.Count; i++ )
		{
			UpdateIcon( inv.GetSlot( i ), slots[i], i );
		}
	}

	private static void UpdateIcon( Entity ent, InventoryIcon inventoryIcon, int i )
	{
		if ( ent == null )
		{
			inventoryIcon.Clear();
			return;
		}

		if ( Local.Pawn is not Player player )
		{
			return;
		}

		inventoryIcon.TargetEnt = ent;
		inventoryIcon.Label.Text = Language.TryGetPhrase( ent.ClassInfo.Name );
		inventoryIcon.SetClass( "active", player.ActiveChild == ent );
	}

	[Event( "buildinput" )]
	public void ProcessClientInput( InputBuilder input )
	{
		var player = Local.Pawn as Player;
		if ( player == null )
		{
			return;
		}

		var inventory = player.Inventory;
		if ( inventory == null )
		{
			return;
		}

		if ( player.ActiveChild is PhysGun physgun && physgun.BeamActive )
		{
			return;
		}

		if ( input.Pressed( InputButton.Slot1 ) )
		{
			SetActiveSlot( input, inventory, 0 );
		}

		if ( input.Pressed( InputButton.Slot2 ) )
		{
			SetActiveSlot( input, inventory, 1 );
		}

		if ( input.Pressed( InputButton.Slot3 ) )
		{
			SetActiveSlot( input, inventory, 2 );
		}

		if ( input.Pressed( InputButton.Slot4 ) )
		{
			SetActiveSlot( input, inventory, 3 );
		}

		if ( input.Pressed( InputButton.Slot5 ) )
		{
			SetActiveSlot( input, inventory, 4 );
		}

		if ( input.Pressed( InputButton.Slot6 ) )
		{
			SetActiveSlot( input, inventory, 5 );
		}

		if ( input.Pressed( InputButton.Slot7 ) )
		{
			SetActiveSlot( input, inventory, 6 );
		}

		if ( input.Pressed( InputButton.Slot8 ) )
		{
			SetActiveSlot( input, inventory, 7 );
		}

		if ( input.Pressed( InputButton.Slot9 ) )
		{
			SetActiveSlot( input, inventory, 8 );
		}

		if ( input.MouseWheel != 0 )
		{
			SwitchActiveSlot( input, inventory, -input.MouseWheel );
		}
	}

	private static void SetActiveSlot( InputBuilder input, IBaseInventory inventory, int i )
	{
		if ( Local.Pawn is not Player player )
		{
			return;
		}

		var ent = inventory.GetSlot( i );
		if ( player.ActiveChild == ent )
		{
			return;
		}

		if ( ent == null )
		{
			return;
		}

		input.ActiveChild = ent;
	}

	private static void SwitchActiveSlot( InputBuilder input, IBaseInventory inventory, int idelta )
	{
		var count = inventory.Count();
		if ( count == 0 )
		{
			return;
		}

		var slot = inventory.GetActiveSlot();
		var nextSlot = slot + idelta;

		while ( nextSlot < 0 )
		{
			nextSlot += count;
		}

		while ( nextSlot >= count )
		{
			nextSlot -= count;
		}

		SetActiveSlot( input, inventory, nextSlot );
	}
}
