using System.Linq;
using Sandbox;

namespace SandboxGame.Entities;

[Library( Constants.Entity.DirectionalGravity, Spawnable = true )]
public class DirectionalGravity : Prop
{
	private bool enabled;

	public override void Spawn()
	{
		base.Spawn();

		DeleteOthers();

		SetModel( "models/arrow.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

		enabled = true;
	}

	private void DeleteOthers()
	{
		// Only allow one of these to be spawned at a time
		foreach ( var ent in All.OfType<DirectionalGravity>()
			         .Where( x => x.IsValid() && x != this ) )
		{
			ent.Delete();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( IsServer )
		{
			Map.Physics.Gravity = Vector3.Down * 800f;
			All.OfType<ModelEntity>().ToList().ForEach( x =>
			{
				if ( x.PhysicsBody is not null )
				{
					x.PhysicsBody.Sleeping = false;
				}
			} );
		}

		enabled = false;
	}

	[Event.Physics.PostStep]
	protected void UpdateGravity()
	{
		if ( !IsServer )
		{
			return;
		}

		if ( !enabled )
		{
			return;
		}

		if ( !this.IsValid() )
		{
			return;
		}

		var gravity = Rotation.Down * 800.0f;
		if ( gravity != Map.Physics.Gravity )
		{
			Map.Physics.Gravity = gravity;
			All.OfType<ModelEntity>().ToList().ForEach( x =>
			{
				if ( x.PhysicsBody is not null )
				{
					x.PhysicsBody.Sleeping = false;
				}
			} );
		}
	}
}
