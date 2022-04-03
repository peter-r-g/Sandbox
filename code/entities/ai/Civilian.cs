using Sandbox;
using SandboxGame.Entities.AI;
using SandboxGame.Vendor.FluentBehaviourTree;

namespace SandboxGame.Entities;

[Library( Constants.Entity.Civilian, Spawnable = true )]
public class Civilian : CitizenAi
{
	private const int FleeSpeed = 300;
	private const int FleeDistance = 1000;
	
	public override void Spawn()
	{
		base.Spawn();
		
		var defaultSpeed = Rand.Float( 100, 150 );
		Speed = defaultSpeed;
		
		var rand = Rand.Int( 0, 2 );
		if ( rand == 0 )
			ClothingOutfits.CivilianMale( this );
		else
			ClothingOutfits.CivilianFemale( this );

		BehaviourTree = new BehaviourTreeBuilder<IAiContext>().Selector( "Movement" )
			.Sequence( "FleeAndHide" )
				.Condition( "ShouldFleeAndHide", _ => IsHurt )
				.Do( "FleeFromAttacker", _ =>
				{
					Speed = FleeSpeed;
					var positionDif = LastAttacker.Position - Position;
					var dirAwayFromEnemy = positionDif.Normal.WithZ( 0 ) * -1;
					NavDestination = Position + dirAwayFromEnemy * FleeDistance;

					if ( !(positionDif.Length > FleeDistance) )
					{
						return BehaviourTreeStatus.Running;
					}

					StopMoving();
					Speed = defaultSpeed;
					return BehaviourTreeStatus.Success;
				} )
				.Do( "HideFromAttacker", _ =>
				{
					var tr = Trace.Ray( LastAttacker.EyePosition, NavDestination )
						.UseHitboxes()
						.HitLayer( CollisionLayer.Debris )
						.Ignore( LastAttacker )
						.Run();

					if ( tr.Hit && tr.Entity == this )
					{
						var result = GetHidingSpot( Position, LastAttacker, 1, 1000 );
						if ( result.Success )
							NavDestination = result.Position;
					}

					return NavMovementState == NavMovementState.Completed
						? BehaviourTreeStatus.Success
						: BehaviourTreeStatus.Running;
				} )
			.End()
			.Splice( CommonTrees.Movement.Wander( Rand.Int( 0, 5 ), Rand.Int( 100, 500 ), Rand.Int( 500, 1000 ) ) )
			.End().Build();
	}

	private HidingSpotResult GetHidingSpot( Vector3 position, Entity hideFrom, int minRadius, int maxRadius, int attempts = 5 )
	{
		for ( var i = 0; i < attempts; i++ )
		{
			var possibleDestination = NavMesh.GetPointWithinRadius( Position, minRadius, maxRadius );
			if ( !possibleDestination.HasValue )
				continue;

			var tr = Trace.Ray( hideFrom.EyePosition, possibleDestination.Value )
				.UseHitboxes()
				.HitLayer( CollisionLayer.Debris )
				.Ignore( hideFrom )
				.Run();

			if ( tr.Hit && tr.Entity == this )
				continue;

			Log.Info( tr.Hit );
			Log.Info( tr.Entity );
			return new HidingSpotResult( true, possibleDestination.Value );
		}

		return new HidingSpotResult( false, Vector3.Zero );
	}

	private readonly struct HidingSpotResult
	{
		public readonly Vector3 Position;
		public readonly bool Success;

		public HidingSpotResult( bool success, Vector3 hidingSpot )
		{
			Success = success;
			Position = hidingSpot;
		}
	}
}
