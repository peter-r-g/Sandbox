using Sandbox;
using Sandbox.UI;
using SandboxGame.Entities.VR;

namespace SandboxGame.Entities;

public partial class SandboxPlayer
{
	// Hands
	[Net] public VrHandEntity LeftHandEntity { get; set; }
	[Net] public VrHandEntity RightHandEntity { get; set; }
	[Net, Predicted] public TimeSince TimeSinceSnap { get; protected set; } = -1;
	
	// Snap Rotation
	public virtual float SnapRotationDelay => 0.25f;
	public virtual float SnapRotationAngle => 45f;
	public virtual float SnapRotationDeadzone => 0.2f;

	public WorldInput WorldInput = new();
	
	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );
		
		ClientSimulateHands();
	}

	public override void BuildInput( InputBuilder input )
	{
		base.BuildInput( input );

		if ( !Input.VR.IsActive )
		{
			return;
		}
		
		var pos = RightHandEntity.Position;
		var rot = RightHandEntity.Rotation;
		var ray = new Ray( pos, rot.Forward );

		WorldInput.Ray = ray;
		WorldInput.MouseLeftPressed = Input.VR.RightHand.Trigger.Value.AlmostEqual( 1f );
	}

	protected virtual void SimulateHands()
	{
		LeftHandEntity?.Simulate( Client );
		RightHandEntity?.Simulate( Client );
	}
	
	protected virtual void ClientSimulateHands()
	{
		LeftHandEntity?.FrameSimulate( Client );
		RightHandEntity?.FrameSimulate( Client );
	}
	
	protected virtual void SimulateTrackedObjects()
	{
		foreach ( var tracked in Input.VR.TrackedObjects )
		{
			DebugOverlay.Text( tracked.Transform.Position, $"Tracking: {tracked.Type}" );
		}
	}
	
	private void SimulateSnapRotation()
	{
		var yawInput = Input.VR.RightHand.Joystick.Value.x;

		if ( TimeSinceSnap > SnapRotationDelay )
		{
			if ( yawInput > SnapRotationDeadzone )
			{
				Transform = Transform.RotateAround(
					Input.VR.Head.Position.WithZ( Position.z ),
					Rotation.FromAxis( Vector3.Up, -SnapRotationAngle )
				);
				TimeSinceSnap = 0;
			}
			else if ( yawInput < -SnapRotationDeadzone )
			{
				Transform = Transform.RotateAround(
					Input.VR.Head.Position.WithZ( Position.z ),
					Rotation.FromAxis( Vector3.Up, SnapRotationAngle )
				);
				TimeSinceSnap = 0;
			}

		}

		if ( yawInput > -SnapRotationDeadzone && yawInput < SnapRotationDeadzone )
		{
			TimeSinceSnap = SnapRotationDelay + 0.1f;
		}
	}
}
