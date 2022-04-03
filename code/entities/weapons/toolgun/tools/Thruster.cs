﻿using Sandbox;
using SandboxGame.Entities;

namespace SandboxGame.Tools;

[Library( Constants.Tool.Thruster, Group = "construction" )]
public class ThrusterTool : BaseTool
{
	private bool massless = true;
	private PreviewEntity previewModel;

	public override void CreatePreviews()
	{
		if ( TryCreatePreview( ref previewModel, "models/thruster/thrusterprojector.vmdl" ) )
		{
			previewModel.RotationOffset = Rotation.FromAxis( Vector3.Right, -90 );
		}
	}

	protected override bool IsPreviewTraceValid( TraceResult tr )
	{
		if ( !base.IsPreviewTraceValid( tr ) )
		{
			return false;
		}

		return tr.Entity is not ThrusterEntity;
	}

	public override void Simulate()
	{
		if ( !Host.IsServer )
		{
			return;
		}

		using ( Prediction.Off() )
		{
			if ( Input.Pressed( InputButton.Attack2 ) )
			{
				massless = !massless;
			}

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

			if ( tr.Entity is ThrusterEntity )
			{
				// TODO: Set properties

				return;
			}

			var ent = new ThrusterEntity
			{
				Position = tr.EndPosition,
				Rotation = Rotation.LookAt( tr.Normal, dir ) * Rotation.From( new Angles( 90, 0, 0 ) ),
				PhysicsEnabled = !attached,
				EnableSolidCollisions = !attached,
				TargetBody = attached ? tr.Body : null,
				Massless = massless
			};

			if ( attached )
			{
				ent.SetParent( tr.Body.GetEntity(), tr.Body.GroupName );
			}

			ent.SetModel( "models/thruster/thrusterprojector.vmdl" );
			UndoHandler.Register( Owner, ent );
		}
	}
}