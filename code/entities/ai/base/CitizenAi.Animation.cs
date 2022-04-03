using System;
using Sandbox;
using SandboxGame.Weapons;

namespace SandboxGame.Entities.AI;

public partial class CitizenAi
{
	private float duck;
	
	public virtual void SimulateAnimation()
	{
		//var idealRotation = Rotation.LookAt( Input.Rotation.Forward.WithZ( 0 ), Vector3.Up );

		//DoRotation( idealRotation );
		DoWalk();
		
		var sitting = Tags.Has( "sitting" );
		var noclip = Tags.Has( "noclip" ) && !sitting;

		SetAnimParameter( "b_grounded", GroundEntity != null || noclip || sitting );
		SetAnimParameter( "b_noclip", noclip );
		SetAnimParameter( "b_sit", sitting );
		SetAnimParameter( "b_swim", WaterLevel > 0.5f && !sitting );

		var aimPos = EyePosition + EyeRotation.Forward * 200;
		
		SetLookAt( "aim_eyes", aimPos );
		SetLookAt( "aim_head", aimPos );
		SetLookAt( "aim_body", aimPos );

		duck = Tags.Has( "ducked" )
			? duck.LerpTo( 1.0f, Time.Delta * 10.0f )
			: duck.LerpTo( 0.0f, Time.Delta * 5.0f );

		SetAnimParameter( "duck", duck );

		if ( ActiveChild is Weapon carry )
		{
			carry.SimulateAnimator( this );
		}
		else
		{
			SetAnimParameter( "holdtype", 0 );
			SetAnimParameter( "aim_body_weight", 0.5f );
		}
	}
	
	private void DoWalk()
	{
		// Wish Speed
		{
			var dir = inputVelocity;
			var forward = Rotation.Forward.Dot( dir );
			var sideward = Rotation.Right.Dot( dir );

			var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

			SetAnimParameter( "wish_direction", angle );
			SetAnimParameter( "wish_speed", inputVelocity.Length );
			SetAnimParameter( "wish_groundspeed", inputVelocity.WithZ( 0 ).Length );
			SetAnimParameter( "wish_y", sideward );
			SetAnimParameter( "wish_x", forward );
		}
		
		// Move Speed
		{
			var dir = Velocity;
			var forward = Rotation.Forward.Dot( dir );
			var sideward = Rotation.Right.Dot( dir );

			var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

			SetAnimParameter( "move_direction", angle );
			SetAnimParameter( "move_speed", Velocity.Length );
			SetAnimParameter( "move_groundspeed", Velocity.WithZ( 0 ).Length );
			SetAnimParameter( "move_y", sideward );
			SetAnimParameter( "move_x", forward );
			SetAnimParameter( "move_z", Velocity.z );
		}
	}

	private void SetLookAt( string name, Vector3 position )
	{
		var localPos = (position - EyePosition) * Rotation.Inverse;
		SetAnimParameter( name, localPos );
	}
}
