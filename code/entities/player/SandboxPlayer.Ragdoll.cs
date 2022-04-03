using Sandbox;

namespace SandboxGame.Entities;

public partial class SandboxPlayer
{
	private void BecomeRagdoll( Vector3 velocity, DamageFlags damageFlags, Vector3 forcePos, Vector3 force,
		int bone )
	{
		var ent = new ModelEntity
		{
			Position = Position,
			Rotation = Rotation,
			Scale = Scale,
			MoveType = MoveType.Physics,
			UsePhysicsCollision = true,
			EnableAllCollisions = true,
			CollisionGroup = CollisionGroup.Debris,
			EnableHitboxes = true,
			SurroundingBoundsMode = SurroundingBoundsType.Physics,
			RenderColor = RenderColor,
			EnableDrawing = true
		};
		ent.SetModel( GetModelName() );
		ent.CopyBonesFrom( this );
		ent.CopyBodyGroups( this );
		ent.CopyMaterialGroup( this );
		ent.TakeDecalsFrom( this );
		ent.SetInteractsAs( CollisionLayer.Debris );
		ent.SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );
		ent.SetInteractsExclude( CollisionLayer.Player | CollisionLayer.Debris );
		ent.PhysicsGroup.Velocity = velocity;
		
		foreach ( var child in Children )
		{
			if ( !child.Tags.Has( "clothes" ) || child is not ModelEntity e )
			{
				continue;
			}

			var model = e.GetModelName();
			var clothing = new ModelEntity {RenderColor = e.RenderColor};
			clothing.SetModel( model );
			clothing.SetParent( ent, true );
			clothing.CopyBodyGroups( e );
			clothing.CopyMaterialGroup( e );
		}

		if ( damageFlags.HasFlag( DamageFlags.Bullet ) ||
		     damageFlags.HasFlag( DamageFlags.PhysicsImpact ) )
		{
			var body = bone > 0 ? ent.GetBonePhysicsBody( bone ) : null;

			if ( body != null )
			{
				body.ApplyImpulseAt( forcePos, force * body.Mass );
			}
			else
			{
				ent.PhysicsGroup.ApplyImpulse( force );
			}
		}

		if ( damageFlags.HasFlag( DamageFlags.Blast ) )
		{
			if ( ent.PhysicsGroup != null )
			{
				ent.PhysicsGroup.AddVelocity( (Position - (forcePos + Vector3.Down * 100.0f)).Normal *
				                              (force.Length * 0.2f) );
				var angularDir = (Rotation.FromYaw( 90 ) * force.WithZ( 0 ).Normal).Normal;
				ent.PhysicsGroup.AddAngularVelocity( angularDir * (force.Length * 0.02f) );
			}
		}

		Corpse = ent;
		ent.DeleteAsync( 10.0f );
	}
}
