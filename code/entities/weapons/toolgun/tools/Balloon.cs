using System.IO;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using SandboxGame.Entities;
using SandboxGame.UI;

namespace SandboxGame.Tools;

[Library( Constants.Tool.Balloon, Group = "construction" )]
public class BalloonTool : BaseTool
{
	private static readonly string[] balloonModels =
	{
		"models/citizen_props/balloonregular01.vmdl", "models/citizen_props/balloonheart01.vmdl",
		"models/citizen_props/balloontall01.vmdl", "models/citizen_props/balloonears01.vmdl"
	};

	public float BalloonForce = 10;
	public string BalloonModel = "models/citizen_props/balloonregular01.vmdl";

	private PreviewEntity previewModel;
	public float RopeLength = 100;
	public Color Tint = Color.Red;

	protected override bool IsPreviewTraceValid( TraceResult tr )
	{
		if ( !base.IsPreviewTraceValid( tr ) )
		{
			return false;
		}

		if ( tr.Entity is Balloon )
		{
			return false;
		}

		return true;
	}

	public override void CreatePreviews()
	{
		if ( TryCreatePreview( ref previewModel, "models/citizen_props/balloonregular01.vmdl" ) )
		{
			previewModel.RelativeToNormal = false;
		}
	}

	public override void Simulate()
	{
		if ( previewModel.IsValid() )
		{
			previewModel.RenderColor = Tint;
		}

		if ( !Host.IsServer )
		{
			return;
		}

		using ( Prediction.Off() )
		{
			var useRope = Input.Pressed( InputButton.Attack1 );
			if ( !useRope && !Input.Pressed( InputButton.Attack2 ) )
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

			CreateHitEffects( tr.EndPosition );

			if ( tr.Entity is Balloon )
			{
				return;
			}

			var ent = new Balloon {GravityScale = -BalloonForce / 50, Position = tr.EndPosition};

			ent.SetModel( BalloonModel );
			ent.RenderColor = Tint;

			UndoHandler.Register( Owner, ent );

			if ( !useRope )
			{
				return;
			}

			var rope = Particles.Create( "particles/rope.vpcf" );
			rope.SetEntity( 0, ent );

			var attachEnt = tr.Body.IsValid() ? tr.Body.GetEntity() : tr.Entity;
			var attachLocalPos = tr.Body.Transform.PointToLocal( tr.EndPosition ) * (1.0f / tr.Entity.Scale);

			if ( attachEnt.IsWorld )
			{
				rope.SetPosition( 1, attachLocalPos );
			}
			else
			{
				rope.SetEntityBone( 1, attachEnt, tr.Bone, new Transform( attachLocalPos ) );
			}

			var spring = PhysicsJoint.CreateLength( ent.PhysicsBody, tr.Body.WorldPoint( tr.EndPosition ), RopeLength );
			spring.SpringLinear = new PhysicsSpring( 5.0f, 0.7f );
			spring.EnableAngularConstraint = false;
			spring.Collisions = true;


			/*var spring = PhysicsJoint.Spring
				.From( ent.PhysicsBody )
				.To( tr.Body, tr.Body.Transform.PointToLocal( tr.EndPosition ) )
				.WithFrequency( 5.0f )
				.WithDampingRatio( 0.7f )
				.WithReferenceMass( ent.PhysicsBody.Mass )
				.WithMinRestLength( 0 )
				.WithMaxRestLength( RopeLength )
				.WithCollisionsEnabled()
				.Create(); */

			spring.OnBreak += () =>
			{
				rope?.Destroy( true );
				spring.Remove();
			};
		}
	}

	public override void ReadSettings( BinaryReader streamReader )
	{
		Tint = Color.Read( streamReader );
		RopeLength = streamReader.ReadSingle();
		BalloonForce = streamReader.ReadSingle();
		BalloonModel = streamReader.ReadString();
	}


	private void UpdateSettings()
	{
		using ( SettingsWriter writer = new() )
		{
			Tint.Write( writer );
			RopeLength.Write( writer );
			BalloonForce.Write( writer );
			BalloonModel.Write( writer );
		}
	}

	public override Panel MakeSettingsPanel()
	{
		SettingsPanel sPanel = new();

		sPanel.AddChild( new Title( "Balloon Model" ) );
		var modelSelector = sPanel.Add.ModelSelector();
		modelSelector.Models.Add( BalloonModel );

		foreach ( var model in balloonModels )
		{
			modelSelector.AddEntry( model, () =>
			{
				BalloonModel = modelSelector.Models[0];
				UpdateSettings();
			} );
		}

		sPanel.AddChild( new Title( "Balloon Color" ) );
		var cPicker = sPanel.Add.ColorPicker( clr =>
		{
			Tint = clr;
			UpdateSettings();
		} );

		cPicker.ColorHSV = Tint;

		var forceSlider = sPanel.Add.SliderLabeled( "Balloon Force", 0, 100, 1 );
		forceSlider.Value = BalloonForce;

		forceSlider.OnFinalValue = forceValue =>
		{
			BalloonForce = forceValue;
			UpdateSettings();
		};

		var lengthSlider = sPanel.Add.SliderLabeled( "Rope Length", 0, 256, 1 );
		lengthSlider.Value = RopeLength;

		lengthSlider.OnFinalValue = lengthValue =>
		{
			RopeLength = lengthValue;
			UpdateSettings();
		};

		return sPanel;
	}
}
