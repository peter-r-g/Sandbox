using System;
using System.Linq;
using Sandbox;

namespace SandboxGame.Entities.VR;

public partial class VrHandEntity : AnimEntity
{
	[Net] public VrHand Hand { get; set; } = VrHand.Left;
	[Net, Predicted] public bool IsGripping { get; set; } = false;
	[Net, Predicted] public TimeSince TimeSincePickup { get; set; } = -1;

	[Net, Predicted] public ModelEntity HeldObject { get; private set; }
	public virtual float HandRadius => 5f;
	public virtual float PickupCooldown => 1f;

	public FingerData FingerData;

	public Vector3 HoldOffset => Transform.Rotation.Right * 0f + Transform.Rotation.Forward * 0f + Transform.Rotation.Up * -0.1f;
	public Transform HoldTransform => Transform.WithPosition( Transform.Position + HoldOffset );
	
	public Input.VrHand HandInput
	{ 
		get
		{
			return Hand switch
			{
				VrHand.Left => Input.VR.LeftHand,
				VrHand.Right => Input.VR.RightHand,
				_ => throw new Exception( "Invalid hand specified for VRHandEntity.GetInput" )
			};
		}
	}

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "hand" );
		UsePhysicsCollision = true;
	}
	
	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		//ShowDebug();

		// Parse finger data
		FingerData.Parse( HandInput );

		// Bullshit rotation here
		Transform = HandInput.Transform.WithRotation( (HandInput.Transform.Rotation.RotateAroundAxis( Vector3.Right, -45f ) ) );
		IsGripping = HandInput.Grip > 0f;

		if ( Host.IsServer )
		{
			switch (IsGripping)
			{
				case true when !HeldObject.IsValid():
					var entity = FindHoldableObject();

					if ( entity != HeldObject && entity.IsValid() )
					{
						StartHoldingObject( entity as IHoldableEntity );
					}

					break;
				case false when HeldObject.IsValid():
					StopHoldingObject();
					break;
			}

			(HeldObject as IHoldableEntity)?.SimulateHeldObject( this );
		}

		Animate();
	}

	protected virtual void HeldObjectDrop()
	{
		(HeldObject as IHoldableEntity)?.OnDrop( this );
	}

	protected virtual void HeldObjectPickup()
	{
		(HeldObject as IHoldableEntity)?.OnPickup( this );
		TimeSincePickup = 0;
	}

	public void StartHoldingObject( IHoldableEntity obj )
	{
		if ( obj.IsBeingHeld )
			return;

		if ( TimeSincePickup < PickupCooldown )
			return;

		HeldObjectDrop();
		HeldObject = obj as ModelEntity;
		HeldObjectPickup();
	}

	public Entity FindHoldableObject()
	{
		var pos = HoldTransform.Position;

		var ent = FindInSphere( pos, HandRadius )
			.Where( x => x is IHoldableEntity obj && !obj.IsBeingHeld )
			.OrderBy( x => x.Position.Distance( pos ) )
			.FirstOrDefault();

		return ent;
	}

	protected void ShowDebug()
	{
		DebugOverlay.Box( HoldTransform.Position, HoldTransform.Rotation, -1, 1, IsServer ? Color.Red : Color.Green, 0.0f, true );
		DebugOverlay.Text( HoldTransform.Position, $"{HandInput.Joystick.Value}", IsServer ? Color.White : Color.Yellow, 0.0f );
		DebugOverlay.Text( HoldTransform.Position + HoldTransform.Rotation.Down * .5f, $"{HandInput.Grip.Value}", IsServer ? Color.White : Color.Yellow, 0.0f );
	}

	private void Animate()
	{
		SetAnimParameter( "bGrab", true );
		SetAnimParameter( "BasePose", 1 );

		SetAnimParameter( "FingerCurl_Middle", FingerData.Middle );
		SetAnimParameter( "FingerCurl_Ring", FingerData.Ring );
		SetAnimParameter( "FingerCurl_Pinky", FingerData.Pinky );
		SetAnimParameter( "FingerCurl_Index", FingerData.Index );
		SetAnimParameter( "FingerCurl_Thumb", FingerData.Thumb );
	}

	private void StopHoldingObject()
	{
		HeldObjectDrop();
		HeldObject = null;
	}
}

public enum VrHand
{
	Left = 0,
	Right = 1
}

public struct FingerData
{
	public float Index { get; set; }
	public float Middle { get; set; }
	public float Ring { get; set; }
	public float Pinky { get; set; }
	public float Thumb { get; set; }

	public bool IsTriggerDown()
	{
		return Index.AlmostEqual( 1f, 0.1f );
	}

	public void Parse( Input.VrHand input )
	{
		Thumb = input.GetFingerCurl( 0 );
		Index = input.GetFingerCurl( 1 );
		Middle = input.GetFingerCurl( 2 );
		Ring = input.GetFingerCurl( 3 );
		Pinky = input.GetFingerCurl( 4 );
	}

	public void DebugLog()
	{
		string Realm = Host.IsServer ? "Server" : "Client";
		Log.Info( $"{Realm}: {Thumb}, {Index}, {Middle}, {Ring}, {Pinky}" );
	}
}
