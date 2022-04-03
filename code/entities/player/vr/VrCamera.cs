using Sandbox;

namespace SandboxGame.Entities.VR;

public class VrCamera : FirstPersonCamera
{
	public override void Build( ref CameraSetup setup )
	{
		base.Build( ref setup );

		setup.ZNear = 1f;
	}

	public virtual Vector2 GetJoystickInput( bool isLeft = true )
	{
		var input = isLeft ? Input.VR.LeftHand.Joystick : Input.VR.RightHand.Joystick;

		return new Vector2( input.Value.x, input.Value.y );
	}

	public override void BuildInput( InputBuilder builder )
	{
		var joystick = GetJoystickInput();
		builder.InputDirection.y = -joystick.x;
		builder.InputDirection.x = joystick.y;
	}
}
