using System.Linq;
using Sandbox;
using SandboxGame.Weapons;

namespace SandboxGame.Entities.AI;

public class CitizenInventory : Inventory, IAiInventory
{
	public override Entity Active
	{
		get
		{
			return (Owner as CitizenAi)?.ActiveChild;
		}

		set
		{
			if ( Owner is CitizenAi ai )
			{
				ai.ActiveChild = value;
			}
		}
	}
	
	public CitizenInventory( Entity owner ) : base ( owner ) {}

	public override bool Add( Entity entity, bool makeActive = false )
	{
		var result = base.Add( entity, makeActive );
		if ( result )
			SortItems();

		return result;
	}

	public override bool Drop( Entity ent )
	{
		var result = base.Drop( ent );
		if ( result )
			SortItems();

		return result;
	}

	public override void SortItems()
	{
		List = List.OrderByDescending( x => x is Weapon wep ? wep.AiPriority : -1 ).ToList();
	}

	public virtual bool Contains( string entityClass ) => Get( entityClass ) is not null;

	public virtual Entity Get( string entityClass )
		=> List.FirstOrDefault( item => item.ClassInfo.Name == entityClass );

	public virtual Entity GetNext( int skip = 0 )
	{
		var skipped = 0;
		foreach ( var item in List )
		{
			if ( skipped < skip )
			{
				skipped++;
				continue;
			}

			return item;
		}

		return null;
	}
}
