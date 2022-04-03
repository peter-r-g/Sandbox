namespace SandboxGame.Entities.VR;

public interface IHoldableEntity
{
	bool IsBeingHeld { get; }

	bool SimulateHeldObject( VrHandEntity hand );
	Vector3 GetHoldPosition( VrHandEntity hand );
	Rotation GetHoldRotation( VrHandEntity hand );
	void OnPickup( VrHandEntity hand );
	void OnDrop( VrHandEntity hand );
}
