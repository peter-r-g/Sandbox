using SandboxGame.Vendor.FluentBehaviourTree;
using Sandbox;
using SandboxGame.Debug;

namespace SandboxGame.Entities.AI;

public partial class CitizenAi : AnimEntity
{
	public virtual float MaxHealth => 100;
	public virtual bool DropWeapon => false;
	public virtual bool IsHurt => Health < MaxHealth;
	public virtual bool Idle => Velocity.IsNearlyZero();
	public IAiInventory Inventory { get; protected set; }
	public float Speed { get; protected set; }
	public DamageInfo LastDamage { get; protected set; }
	public Vector3 NavDestination
	{
		get => steer.Target;
		set
		{
			if ( !NavMesh.IsLoaded )
			{
				return;
			}

			var targetPos = NavMesh.GetClosestPoint( value );
			steer.Target = targetPos ?? Position;
			NavMovementState = NavMovementState.Running;
		}
	}
	public NavMovementState NavMovementState { get; protected set; }
	public TimeSince TimeSinceReachedNavDestination { get; protected set; }

	[Net, Predicted] public Entity ActiveChild { get; set; }
	[Predicted] private Entity LastActiveChild { get; set; }

	protected IBehaviourTreeNode<IAiContext> BehaviourTree;
	protected IAiContext Context;

	private NavSteer steer;
	private Vector3 inputVelocity;
	
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen/citizen.vmdl" );
		SetMaterialGroup( Rand.Int( 0, 3 ) );
		SetupPhysicsFromCapsule( PhysicsMotionType.Keyframed, Capsule.FromHeightAndRadius( 72, 8 ) );

		CollisionGroup = CollisionGroup.Player;
		EnableHitboxes = true;
		EyePosition = Position + Vector3.Up * 64;
		Health = MaxHealth;
		Speed = Rand.Float( 100, 300 );
		
		steer = new NavSteer();
		Inventory = new CitizenInventory( this );
		Context = new BaseAiContext( this );
	}

	public override void Simulate( Client cl )
	{
#if DEBUG
		using var a = Profile.Scope( "CitizenAi::Simulate::Base" );
#endif
		base.Simulate( cl );
		
#if DEBUG
		a?.Dispose();
		using var b = Profile.Scope( "CitizenAi::Simulate" );
#endif

		SimulateAnimation();
		SimulateActiveChild( ActiveChild );
	}

	public override void FrameSimulate( Client cl )
	{
#if DEBUG
		using var a = Profile.Scope( "CitizenAi::FrameSimulate" );
#endif
		base.FrameSimulate( cl );
		
		ActiveChild?.FrameSimulate( cl );
	}

	public override void OnChildAdded( Entity child )
	{
		Inventory?.OnChildAdded( child );
	}

	public override void OnChildRemoved( Entity child )
	{
		Inventory?.OnChildRemoved( child );
	}

	public override void TakeDamage( DamageInfo info )
	{
		base.TakeDamage( info );

		LastDamage = info;
	}

	public override void OnKilled()
	{
		BecomeRagdoll( Velocity, LastDamage.Flags, LastDamage.Position, LastDamage.Force,
			GetHitboxBone( LastDamage.HitboxIndex ) );

		EnableAllCollisions = false;
		EnableDrawing = false;
		foreach ( var child in Children )
		{
			child.EnableDrawing = false;
		}

		if ( DropWeapon )
		{
			Inventory.DropActive();
		}
		Inventory.DeleteContents();
		
		base.OnKilled();
	}

	public virtual void StopMoving()
	{
		NavDestination = Position;
		NavMovementState = NavMovementState.Cancelled;
	}

	protected virtual void BecomeRagdoll( Vector3 velocity, DamageFlags damageFlags, Vector3 forcePos, Vector3 force,
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
		
		ent.DeleteAsync( 10.0f );
	}

	protected virtual void SimulateActiveChild( Entity child )
	{
		if ( LastActiveChild != child )
		{
			OnActiveChildChanged( LastActiveChild, child );
			LastActiveChild = child;
		}

		if ( !LastActiveChild.IsValid() )
		{
			return;
		}

		if ( LastActiveChild.IsAuthority )
		{
			LastActiveChild.Simulate( null );
		}
	}

	protected virtual void OnActiveChildChanged( Entity previous, Entity next )
	{
		if ( previous is BaseCarriable previousBc )
		{
			previousBc.ActiveEnd( this, previousBc.Owner != this );
		}

		if ( next is BaseCarriable nextBc )
		{
			nextBc.ActiveStart( this );
		}
	}

	protected virtual void HandleMovement()
	{
		inputVelocity = 0;

		if ( steer != null )
		{
			steer.Tick( Position );

			if ( !steer.Output.Finished )
			{
				inputVelocity = steer.Output.Direction.Normal;
				Velocity = Velocity.AddClamped( inputVelocity * Time.Delta * 500, Speed );
			}
			else if ( NavMovementState != NavMovementState.Completed )
			{
				TimeSinceReachedNavDestination = 0;
				NavMovementState = NavMovementState.Completed;
			}
		}
		
		Move();

		var walkVelocity = Velocity.WithZ( 0 );
		if ( walkVelocity.Length < 0.5f )
		{
			return;
		}

		var turnSpeed = walkVelocity.Length.LerpInverse( 0, 100 );
		var targetRotation = Rotation.LookAt( walkVelocity.Normal, Vector3.Up );
		Rotation = Rotation.Lerp( Rotation, targetRotation, turnSpeed * Time.Delta * 20.0f );
	}
	
	protected virtual void Move()
	{
		var bbox = BBox.FromHeightAndRadius( 64, 4 );

		MoveHelper move = new( Position, Velocity ) {MaxStandableAngle = 50};
		move.Trace = move.Trace.Ignore( this ).Size( bbox );

		if ( !Velocity.IsNearlyZero( 0.001f ) )
		{
			move.TryUnstuck();
			move.TryMoveWithStep( Time.Delta, 30 );
		}
		
		var tr = move.TraceDirection( Vector3.Down * 10.0f );

		if ( move.IsFloor( tr ) )
		{
			GroundEntity = tr.Entity;

			if ( !tr.StartedSolid )
			{
				move.Position = tr.EndPosition;
			}

			if ( inputVelocity.Length > 0 )
			{
				var movement = move.Velocity.Dot( inputVelocity.Normal );
				move.Velocity -= movement * inputVelocity.Normal;
				move.ApplyFriction( tr.Surface.Friction * 10.0f, Time.Delta );
				move.Velocity += movement * inputVelocity.Normal;
			}
			else
			{
				move.ApplyFriction( tr.Surface.Friction * 10.0f, Time.Delta );
			}
		}
		else
		{
			GroundEntity = null;
			move.Velocity += Vector3.Down * 900 * Time.Delta;
		}

		Position = move.Position;
		Velocity = move.Velocity;
	}
	
	[Event.Tick.Server]
	protected virtual void Think()
	{
		using var a = Profile.Scope( "Citizen::Think" );

#if DEBUG
		using var b = Profile.Scope( "CitizenAi::Think::BT" );
#endif
		BehaviourTree.Tick( Context );
#if DEBUG
		b?.Dispose();
#endif
		
#if DEBUG
		using var c = Profile.Scope( "CitizenAi::Think::Movement" );
#endif
		HandleMovement();
#if DEBUG
		c?.Dispose();
#endif
	}
}
