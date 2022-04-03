using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using SandboxGame.Weapons;

namespace SandboxGame;

public class Inventory : BaseInventory
{
	private static readonly Dictionary<Type, int> ToolOrder = new()
	{
		{typeof(PhysGun), 0},
		{typeof(GravGun), 1},
		{typeof(ToolGun), 2},
		{typeof(Flashlight), 3},
		{typeof(Pistol), 4},
		{typeof(Smg), 5},
		{typeof(Shotgun), 6},
		{typeof(Fists), 7}
	};

	public Inventory( Entity owner ) : base( owner ) { }

	public override bool CanAdd( Entity entity )
	{
		if ( !entity.IsValid() )
		{
			return false;
		}

		if ( !base.CanAdd( entity ) )
		{
			return false;
		}

		return !IsCarryingType( entity.GetType() );
	}

	public override bool Add( Entity entity, bool makeActive = false )
	{
		if ( !entity.IsValid() )
		{
			return false;
		}

		return !IsCarryingType( entity.GetType() ) && base.Add( entity, makeActive );
	}
	
	public override bool Drop( Entity ent )
	{
		if ( !Host.IsServer )
		{
			return false;
		}

		if ( !Contains( ent ) )
		{
			return false;
		}

		if ( ent is BaseCarriable carriable )
		{
			carriable.OnCarryDrop( Owner );
		}

		return ent.Parent == null;
	}
	
	public virtual void SortItems()
	{
		List = List.OrderBy( x => ToolOrder.TryGetValue( x.GetType(), out var order ) ? order : 10 ).ToList();
	}

	public bool IsCarryingType( Type t )
	{
		return List.Any( x => x?.GetType() == t );
	}
}
