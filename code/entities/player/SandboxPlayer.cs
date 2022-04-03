using Sandbox;
using SandboxGame.Debug;
using SandboxGame.Entities.VR;
using SandboxGame.Tools;
using SandboxGame.UI;
using SandboxGame.Weapons;

namespace SandboxGame.Entities;

public partial class SandboxPlayer : Player
{
	[Net] public PawnController VehicleController { get; set; }
	[Net] public PawnAnimator VehicleAnimator { get; set; }
	[Net] [Predicted] public CameraMode VehicleCamera { get; set; }
	[Net] [Predicted] public Entity Vehicle { get; set; }
	[Net] [Predicted] private CameraMode MainCamera { get; set; }
	private CameraMode LastCamera { get; set; }
	
	public bool InCameraTool => Inventory.Active is ToolGun tool && tool.CurrentTool is CameraTool;
	/// <summary>
	///     The clothing container is what dresses the citizen
	/// </summary>
	public readonly Clothing.Container Clothing = new();

	private DamageInfo lastDamage;
	private TimeSince timeSinceDropped;
	private TimeSince timeSinceJumpReleased;

	/// <summary>
	///     Default init
	/// </summary>
	public SandboxPlayer()
	{
		Inventory = new Inventory( this );
	}

	/// <summary>
	///     Initialize using this client
	/// </summary>
	public SandboxPlayer( Client cl ) : this()
	{
		// Load clothing from client data
		Clothing.LoadFromClient( cl );
	}

	public override void Spawn()
	{
		if ( !Input.VR.IsActive )
		{
			MainCamera = new FirstPersonCamera();
			LastCamera = MainCamera;
		}
		else
		{
			MainCamera = new VrCamera();
			
			LeftHandEntity = new VrHandEntity
			{
				Hand = VrHand.Left,
				Owner = this
			};
			LeftHandEntity.SetModel( "models/hands/alyx_hand_left.vmdl" );

			RightHandEntity = new VrHandEntity
			{
				Hand = VrHand.Right,
				Owner = this
			};
			RightHandEntity.SetModel( "models/hands/alyx_hand_right.vmdl" );
		}

		base.Spawn();
	}

	public override void Respawn()
	{
		Corpse?.Delete();
		SetModel( "models/citizen/citizen.vmdl" );

		if ( !Input.VR.IsActive )
		{
			Controller = new WalkController();
			MainCamera = LastCamera;

			if ( DevController is NoclipController )
			{
				DevController = null;
			}	
		}
		//else
		//	Controller = new FlyingController();

		Animator = new StandardPlayerAnimator();
		
		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		Clothing.DressEntity( this );

		Inventory.Add( new PhysGun(), true );
		Inventory.Add( new GravGun() );
		Inventory.Add( new ToolGun() );
		Inventory.Add( new Flashlight() );
		Inventory.Add( new Pistol() );
		Inventory.Add( new Smg() );
		Inventory.Add( new Shotgun() );
		Inventory.Add( new Fists() );

		base.Respawn();
	}

	public override void OnKilled()
	{
		if ( lastDamage.Flags.HasFlag( DamageFlags.Vehicle ) )
		{
			Particles.Create( "particles/impact.flesh.bloodpuff-big.vpcf", lastDamage.Position );
			Particles.Create( "particles/impact.flesh-big.vpcf", lastDamage.Position );
			PlaySound( "kersplat" );
		}
		
		BecomeRagdoll( Velocity, lastDamage.Flags, lastDamage.Position, lastDamage.Force,
			GetHitboxBone( lastDamage.HitboxIndex ) );

		Clothing.ClearEntities();
		Controller = null;
		VehicleController = null;
		VehicleAnimator = null;
		VehicleCamera = null;
		Vehicle = null;
		EnableAllCollisions = false;
		
		EnableDrawing = false;
		foreach ( var child in Children )
		{
			child.EnableDrawing = false;
		}

		if ( !Input.VR.IsActive )
		{
			LastCamera = MainCamera;
			MainCamera = new SpectateRagdollCamera();
		}

		Inventory.DropActive();
		Inventory.DeleteContents();
		
		base.OnKilled();
	}

	public override void TakeDamage( DamageInfo info )
	{
		if ( GetHitboxGroup( info.HitboxIndex ) == 1 )
		{
			info.Damage *= 10.0f;
		}

		lastDamage = info;

		TookDamage( lastDamage.Flags, lastDamage.Position, lastDamage.Force );

		base.TakeDamage( info );
	}

	public override PawnController GetActiveController()
	{
		if ( VehicleController != null )
		{
			return VehicleController;
		}

		return DevController ?? base.GetActiveController();
	}

	public override PawnAnimator GetActiveAnimator()
	{
		return VehicleAnimator ?? base.GetActiveAnimator();
	}

	public CameraMode GetActiveCamera()
	{
		return VehicleCamera ?? MainCamera;
	}

	public override void Simulate( Client cl )
	{
#if DEBUG
		using var a = Profile.Scope( "SandboxPlayer::Simulate::Base" );
#endif
		
		base.Simulate( cl );
		
#if DEBUG
		a?.Dispose();
#endif

		if ( MainCamera != null )
		{
			CameraMode = GetActiveCamera();
		}

		if ( IsLocalPawn )
		{
			EnableDrawing = !((GetActiveCamera() is FirstPersonCamera || GetActiveCamera() is VrCamera) && InCameraTool);
		}

		if ( Input.ActiveChild != null )
		{
			ActiveChild = Input.ActiveChild;
		}

		if ( LifeState != LifeState.Alive )
		{
			return;
		}

		if ( VehicleController != null && DevController is NoclipController )
		{
			DevController = null;
		}

		var controller = GetActiveController();
		if ( controller != null )
		{
			EnableSolidCollisions = !controller.HasTag( "noclip" );
		}

		TickPlayerUse();
		SimulateActiveChild( cl, ActiveChild );

		if ( !Input.VR.IsActive && Input.Pressed( InputButton.View ) )
		{
			MainCamera = CameraMode is FirstPersonCamera ? new ThirdPersonCamera() : new FirstPersonCamera();
		}

		if ( Input.Pressed( InputButton.Drop ) )
		{
			var dropped = Inventory.DropActive();
			if ( dropped != null )
			{
				dropped.PhysicsGroup.ApplyImpulse( Velocity + EyeRotation.Forward * 500.0f + Vector3.Up * 100.0f,
					true );
				dropped.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

				timeSinceDropped = 0;
			}
		}

		if ( Input.Pressed( InputButton.Grenade ) )
		{
			var undone = UndoHandler.DoUndo( Client.PlayerId );

			if ( undone == 0 )
			{
				return;
			}

			Notifications.SendUndo( To.Single( Client ), undone );
		}

		if ( Input.Released( InputButton.Jump ) )
		{
			if ( timeSinceJumpReleased < 0.3f )
			{
				Game.Current?.DoPlayerNoclip( cl );
			}

			timeSinceJumpReleased = 0;
		}

		if ( Input.Left != 0 || Input.Forward != 0 )
		{
			timeSinceJumpReleased = 1;
		}

		if ( !Input.VR.IsActive )
		{
			return;
		}

#if DEBUG
		using var c = Profile.Scope( "SandboxPlayer::Simulate::VR" );
#endif
		SimulateTrackedObjects();
		SimulateHands();
		SimulateSnapRotation();
			
		EyePosition = Input.VR.Head.Position;
		EyeRotation = Input.VR.Head.Rotation;
	}

	public override void StartTouch( Entity other )
	{
		if ( timeSinceDropped < 1 )
		{
			return;
		}

		base.StartTouch( other );
	}
	
	public static void EquipWeapon( string weapon )
	{
		Host.AssertClient();
		
		SetInventoryCurrent( weapon );
	}

	public static void EquipToolgunWithTool( string tool )
	{
		Host.AssertClient();

		EquipWeapon( Constants.Weapon.Toolgun );
		ConsoleSystem.Run( Constants.Command.CurrentTool, tool );
	}
	
	[ClientRpc]
	protected virtual void TookDamage( DamageFlags damageFlags, Vector3 forcePos, Vector3 force )
	{
	}

	[ServerCmd]
	public static void SetInventoryCurrent( string entName )
	{
		var target = ConsoleSystem.Caller.Pawn as Player;
		var inventory = target?.Inventory;
		if ( inventory == null )
		{
			return;
		}

		for ( var i = 0; i < inventory.Count(); ++i )
		{
			var slot = inventory.GetSlot( i );
			if ( !slot.IsValid() )
			{
				continue;
			}

			if ( !slot.ClassInfo.IsNamed( entName ) )
			{
				continue;
			}

			inventory.SetActiveSlot( i, false );

			break;
		}
	}
}
