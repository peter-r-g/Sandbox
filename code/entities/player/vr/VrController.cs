using Sandbox;

namespace SandboxGame.Entities.VR;

public partial class VrController : PawnController
{
	[Net] public float MaxWalkSpeed { get; set; } = 125f;

	public override void Simulate()
	{
		base.Simulate();

		SimulateMovement();
	}

	public virtual void SimulateMovement()
	{
		var inputRotation = Input.VR.Head.Rotation.Angles().WithPitch( 0 ).ToRotation();

		Velocity = Velocity.AddClamped( inputRotation * new Vector3( Input.Forward, Input.Left, 0 ) * MaxWalkSpeed * 5 * Time.Delta, MaxWalkSpeed );
		Velocity = Velocity.Approach( 0, Time.Delta * MaxWalkSpeed * 3 );

		// Ensure we're on the floor
		Velocity = Velocity.WithZ( -160 );

		//
		// Move helper traces and slides along surfaces for us
		//
		var helper = new MoveHelper( Position, Velocity );
		helper.Trace = helper.Trace.Size( 20 );

		helper.TryUnstuck();
		helper.TryMoveWithStep( Time.Delta, 30.0f );

		Position = helper.Position;
		Velocity = helper.Velocity;
	}
}
