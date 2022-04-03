using Sandbox;
using SandboxGame.Entities;

namespace SandboxGame.Tools;

[Library( Constants.Tool.Wheel, Group = "construction" )]
public class WheelTool : BaseTool
{
	private PreviewEntity previewModel;

	protected override bool IsPreviewTraceValid( TraceResult tr )
	{
		if ( !base.IsPreviewTraceValid( tr ) )
		{
			return false;
		}

		return tr.Entity is not WheelEntity;
	}

	public override void CreatePreviews()
	{
		if ( TryCreatePreview( ref previewModel, "models/citizen_props/wheel01.vmdl" ) )
		{
			previewModel.RotationOffset = Rotation.FromAxis( Vector3.Up, 90 );
		}
	}

	public override void Simulate()
	{
		if ( !Host.IsServer )
		{
			return;
		}

		using ( Prediction.Off() )
		{
			if ( !Input.Pressed( InputButton.Attack1 ) )
			{
				return;
			}

			var startPos = Owner.EyePosition;
			var dir = Owner.EyeRotation.Forward;

			var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
				.Ignore( Owner )
				.Run();

			if ( !tr.Hit )
			{
				return;
			}

			if ( !tr.Entity.IsValid() )
			{
				return;
			}

			var attached = !tr.Entity.IsWorld && tr.Body.IsValid() && tr.Body.PhysicsGroup != null &&
			               tr.Body.GetEntity().IsValid();

			if ( attached && tr.Entity is not Prop )
			{
				return;
			}

			CreateHitEffects( tr.EndPosition );

			if ( tr.Entity is WheelEntity )
			{
				// TODO: Set properties

				return;
			}

			var ent = new WheelEntity
			{
				Position = tr.EndPosition,
				Rotation = Rotation.LookAt( tr.Normal ) * Rotation.From( new Angles( 0, 90, 0 ) )
			};

			ent.SetModel( "models/citizen_props/wheel01.vmdl" );
			ent.Joint = PhysicsJoint.CreateHinge( ent.PhysicsBody, tr.Body, tr.EndPosition, tr.Normal );
			ent.PhysicsBody.Mass = tr.Body.Mass;

			UndoHandler.Register( Owner, ent );
		}
	}
}
