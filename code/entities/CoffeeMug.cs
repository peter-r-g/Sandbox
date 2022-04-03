using Sandbox;
using SandboxGame.Entities.VR;

namespace SandboxGame.Entities;

[Library( Constants.Entity.CoffeeMug, Spawnable = true )]
public class CoffeeMug : ModelEntity, IHoldableEntity
{
	public bool IsBeingHeld { get; private set; }
	
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen_props/coffeemug01.vmdl" );

		MoveType = MoveType.Physics;
		PhysicsEnabled = true;
		UsePhysicsCollision = true;
	}
	
	public bool SimulateHeldObject( VrHandEntity hand )
	{
		Velocity = hand.Velocity;
		BaseVelocity = hand.BaseVelocity;

		return true;
	}

	public Vector3 GetHoldPosition( VrHandEntity hand )
	{
		return hand.HoldTransform.Position;
	}

	public Rotation GetHoldRotation( VrHandEntity hand )
	{
		return hand.HoldTransform.Rotation;
	}
	
	public void OnPickup( VrHandEntity hand )
	{
		IsBeingHeld = true;
		Parent = hand;
		Position = GetHoldPosition( hand );
		Rotation = GetHoldRotation( hand );
	}

	public void OnDrop( VrHandEntity hand )
	{
		IsBeingHeld = false;
		Velocity *= 2f;
		Parent = null;
	}
}
