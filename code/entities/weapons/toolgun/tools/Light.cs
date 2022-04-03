using System.IO;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using SandboxGame.Entities;
using SandboxGame.UI;

namespace SandboxGame.Tools;

[Library( Constants.Tool.Light, Group = "construction" )]
public class LightTool : BaseTool
{
	private Color Color = Color.White;
	private float Range = 128;
	private float RopeLength = 100;
	
	private PreviewEntity previewModel;
	private const string Model = "models/light/light_tubular.vmdl";
	
	protected override bool IsPreviewTraceValid( TraceResult tr )
	{
		if ( !base.IsPreviewTraceValid( tr ) )
		{
			return false;
		}

		return tr.Entity is not LightEntity;
	}

	public override void CreatePreviews()
	{
		if ( !TryCreatePreview( ref previewModel, Model ) )
		{
			return;
		}

		previewModel.RelativeToNormal = false;
		previewModel.OffsetBounds = true;
		previewModel.PositionOffset = -previewModel.CollisionBounds.Center;
	}

	public override void Simulate()
	{
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

			if ( !tr.Hit || !tr.Entity.IsValid() )
			{
				return;
			}

			CreateHitEffects( tr.EndPosition );

			if ( tr.Entity is LightEntity )
			{
				// TODO: Set properties

				return;
			}

			var light = new LightEntity
			{
				Enabled = true,
				DynamicShadows = false,
				Range = Range,
				Falloff = 1.0f,
				LinearAttenuation = 0.0f,
				QuadraticAttenuation = 1.0f,
				Brightness = 1,
				Color = Color
			};

			light.UseFogNoShadows();
			light.SetModel( Model );
			light.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
			light.Position = tr.EndPosition + -light.CollisionBounds.Center +
			                 tr.Normal * light.CollisionBounds.Size * 0.5f;
			UndoHandler.Register( Owner, light );

			if ( !useRope )
			{
				return;
			}

			var rope = Particles.Create( "particles/rope.vpcf" );
			rope.SetEntity( 0, light, Vector3.Down * 6.5f ); // Should be an attachment point

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

			var spring = PhysicsJoint.CreateLength( light.PhysicsBody.LocalPoint( Vector3.Down * 6.5f ),
				tr.Body.WorldPoint( tr.EndPosition ), RopeLength );
			spring.SpringLinear = new PhysicsSpring( 5.0f, 0.7f );
			spring.EnableAngularConstraint = false;
			spring.Collisions = true;

			spring.OnBreak += () =>
			{
				rope.Destroy( true );
				spring.Remove();
			};
		}
	}

	public override void ReadSettings( BinaryReader streamReader )
	{
		Color = Color.Read( streamReader );
		Range = streamReader.ReadSingle();
		RopeLength = streamReader.ReadSingle();
	}

	private void UpdateSettings()
	{
		using SettingsWriter writer = new();
		Color.Write( writer );
		Range.Write( writer );
		RopeLength.Write( writer );
	}

	public override Panel MakeSettingsPanel()
	{
		SettingsPanel sPanel = new();
		sPanel.AddChild( new Title( "Light Color" ) );

		_ = sPanel.Add.ColorPicker( clr =>
		{
			Color = clr;

			UpdateSettings();
		} );

		var rangeSlider = sPanel.Add.SliderLabeled( "Range", 32, 1024, 1 );
		rangeSlider.Value = Range;

		rangeSlider.OnFinalValue = range =>
		{
			Range = range;
			UpdateSettings();
		};

		var ropeSlider = sPanel.Add.SliderLabeled( "Rope Length", 0, 256, 1 );
		ropeSlider.Value = RopeLength;

		ropeSlider.OnFinalValue = ropeSlider =>
		{
			RopeLength = ropeSlider;
			UpdateSettings();
		};
		
		return sPanel;
	}
}
