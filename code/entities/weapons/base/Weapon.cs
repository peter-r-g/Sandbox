using Sandbox;
using Sandbox.ScreenShake;
using SandboxGame.Entities;
using SandboxGame.Entities.AI;

namespace SandboxGame.Weapons;

public partial class Weapon : BaseCarriable, IUse
{
	public virtual float PrimaryRate => 5.0f;
	public virtual float SecondaryRate => 15.0f;
	public virtual float ReloadTime => 3.0f;
	public virtual int MaxClipSize => int.MinValue;
	public virtual int AiPriority => 0;
	public virtual bool AutoReload => false;
	public virtual bool Automatic => false;

	public PickupTrigger PickupTrigger { get; protected set; }
	
	[Net, Predicted] public TimeSince TimeSincePrimaryAttack { get; set; }

	[Net, Predicted] public TimeSince TimeSinceSecondaryAttack { get; set; }

	[Net] [Predicted] public TimeSince TimeSinceReload { get; set; }

	[Net] [Predicted] public bool IsReloading { get; set; }

	[Net] [Predicted] public TimeSince TimeSinceDeployed { get; set; }

	[Net] [Predicted] public int ClipSize { get; set; }
	
	public override void Spawn()
	{
		base.Spawn();
		
		CollisionGroup = CollisionGroup.Weapon; // so players touch it as a trigger but not as a solid
		SetInteractsAs( CollisionLayer.Debris ); // so player movement doesn't walk into it

		PickupTrigger = new PickupTrigger
		{
			Parent = this, Position = Position, EnableTouch = true, EnableSelfCollisions = false,
			PhysicsBody = {AutoSleep = false}
		};

		ClipSize = MaxClipSize;
	}
	
	public override void CreateViewModel()
	{
		Host.AssertClient();

		if ( string.IsNullOrEmpty( ViewModelPath ) )
		{
			return;
		}

		ViewModelEntity = new ViewModel {Position = Position, Owner = Owner, EnableViewmodelRendering = true};
		ViewModelEntity.SetModel( ViewModelPath );
	}
	
	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		TimeSinceDeployed = 0;
	}
	
	public override void Simulate( Client owner )
	{
		if ( TimeSinceDeployed < 0.6f )
		{
			return;
		}
		
		if ( IsReloading && TimeSinceReload > ReloadTime )
		{
			OnReloadFinish();
		}

		if ( Owner is CitizenAi || IsReloading )
		{
			return;
		}

		if ( CanReload( true ) )
		{
			Reload();
		}

		//
		// Reload could have changed our owner
		//
		if ( !Owner.IsValid() )
			return;

		if ( CanPrimaryAttack( true ) )
		{
			using ( LagCompensation() )
			{
				AttackPrimary();
			}
		}

		//
		// AttackPrimary could have changed our owner
		//
		if ( !Owner.IsValid() )
			return;

		if ( CanSecondaryAttack( true ) )
		{
			using ( LagCompensation() )
			{
				AttackSecondary();
			}
		}
	}
	
	public virtual bool CanPrimaryAttack( bool checkInput )
	{
		if ( !Owner.IsValid() || checkInput &&
		    (Automatic && !Input.Down( InputButton.Attack1 ) || !Automatic && !Input.Pressed( InputButton.Attack1 )) ) return false;
		if ( PrimaryRate <= 0 ) return true;
		
		return TimeSincePrimaryAttack > 1 / PrimaryRate;
	}

	public virtual void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
	}
	
	public virtual bool CanSecondaryAttack( bool checkInput )
	{
		if ( !Owner.IsValid() || checkInput && !Input.Down( InputButton.Attack2 ) ) return false;
		if ( SecondaryRate <= 0 ) return true;

		return TimeSinceSecondaryAttack > 1 / SecondaryRate;
	}

	public virtual void AttackSecondary()
	{
		TimeSinceSecondaryAttack = 0;
	}
	
	public virtual bool CanReload( bool checkInput )
	{
		if ( !Owner.IsValid() || checkInput && !Input.Down( InputButton.Reload ) ) return false;

		return !IsReloading && ClipSize < MaxClipSize;
	}
	
	public virtual void Reload()
	{
		TimeSinceReload = 0;
		IsReloading = true;

		(Owner as AnimEntity)?.SetAnimParameter( "b_reload", true );

		StartReloadEffects();
	}

	public virtual void OnReloadFinish()
	{
		ClipSize = MaxClipSize;
		IsReloading = false;
	}

	public virtual void SimulateAnimator( AnimEntity animEntity )
	{
		
	}

	public virtual bool TakeAmmo( int amount = 1 )
	{
		if ( ClipSize == int.MinValue )
		{
			return true;
		}

		if ( ClipSize < amount )
		{
			return false;
		}

		ClipSize -= amount;
		return true;
	}
	
	public bool OnUse( Entity user )
	{
		if ( Owner != null )
		{
			return false;
		}

		if ( !user.IsValid() )
		{
			return false;
		}

		user.StartTouch( this );

		return false;
	}

	public bool IsUsable( Entity user )
	{
		var player = user as Player;
		if ( Owner != null )
		{
			return false;
		}

		if ( player.Inventory is Inventory inventory )
		{
			return inventory.CanAdd( this );
		}

		return true;
	}

	[ClientRpc]
	public virtual void StartReloadEffects()
	{
		ViewModelEntity?.SetAnimParameter( "reload", true );

		// TODO - player third person model reload
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		if ( IsLocalPawn )
		{
			_ = new Perlin();
		}

		ViewModelEntity?.SetAnimParameter( "fire", true );
		CrosshairPanel?.CreateEvent( "fire" );
	}
}
