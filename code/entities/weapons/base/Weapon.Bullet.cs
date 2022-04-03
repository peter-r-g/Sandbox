using System.Collections.Generic;
using Sandbox;

namespace SandboxGame.Weapons;

public partial class Weapon
{
	/// <summary>
	/// Does a trace from start to end, does bullet impact effects. Coded as an IEnumerable so you can return multiple
	/// hits, like if you're going through layers or ricocet'ing or something.
	/// </summary>
	public virtual IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		bool InWater = Map.Physics.IsPointWater( start );

		var tr = Trace.Ray( start, end )
			.UseHitboxes()
			.HitLayer( CollisionLayer.Water, !InWater )
			.HitLayer( CollisionLayer.Debris )
			.Ignore( Owner )
			.Ignore( this )
			.Size( radius )
			.Run();

		if ( tr.Hit )
			yield return tr;

		//
		// Another trace, bullet going through thin material, penetrating water surface?
		//
	}

	/// <summary>
	///     Shoot a single bullet
	/// </summary>
	public virtual void ShootBullet( Vector3 pos, Vector3 dir, float spread, float force, float damage,
		float bulletSize, int bulletNumber = 0 )
	{
		//Sync bullet random cones
		Rand.SetSeed( bulletNumber + Time.Tick );

		var forward = dir;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
		forward = forward.Normal;

		//
		// ShootBullet is coded in a way where we can have bullets pass through shit
		// or bounce off shit, in which case it'll return multiple results
		//
		foreach ( var tr in TraceBullet( pos, pos + forward * 5000, bulletSize ) )
		{
			// Personal taste, I hate the random smoke clouds in the air
			if ( tr.Hit )
			{
				tr.Surface.DoBulletImpact( tr );
			}

			if ( !IsServer )
			{
				continue;
			}

			if ( !tr.Entity.IsValid() )
			{
				continue;
			}

			var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * force, damage )
				.UsingTraceResult( tr )
				.WithAttacker( Owner )
				.WithWeapon( this );

			tr.Entity.TakeDamage( damageInfo );
		}
	}

	/// <summary>
	///     Shoot a single bullet from owners view point
	/// </summary>
	public virtual void ShootBullet( float spread, float force, float damage, float bulletSize )
	{
		ShootBullet( Owner.EyePosition, Owner.EyeRotation.Forward, spread, force, damage, bulletSize );
	}

	/// <summary>
	///     Shoot a multiple bullets from owners view point
	/// </summary>
	public virtual void ShootBullets( int numBullets, float spread, float force, float damage, float bulletSize )
	{
		var pos = Owner.EyePosition;
		var dir = Owner.EyeRotation.Forward;

		for ( var i = 0; i < numBullets; i++ )
		{
			ShootBullet( pos, dir, spread, force / numBullets, damage, bulletSize, i );
		}
	}
}
