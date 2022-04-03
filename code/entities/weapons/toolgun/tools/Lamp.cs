using System;
using System.IO;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using SandboxGame.Entities;
using SandboxGame.UI;

namespace SandboxGame.Tools;

[Library( Constants.Tool.Lamp, Group = "construction" )]
public class LampTool : BaseTool
{
	public Color Color = Color.White;
	public float Fov = 45;
	private PreviewEntity previewModel;
	public float Range = 512;

	private string Model => "models/torch/torch.vmdl";

	protected override bool IsPreviewTraceValid( TraceResult tr )
	{
		if ( !base.IsPreviewTraceValid( tr ) )
		{
			return false;
		}

		return tr.Entity is not LampEntity;
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
			if ( !Input.Pressed( InputButton.Attack1 ) )
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

			if ( tr.Entity is LampEntity lamp )
			{
				// TODO: Set properties

				lamp.Flicker = !lamp.Flicker;

				return;
			}

			lamp = new LampEntity
			{
				Enabled = true,
				DynamicShadows = true,
				Range = Range,
				Falloff = 1.0f,
				LinearAttenuation = 0.0f,
				QuadraticAttenuation = 1.0f,
				InnerConeAngle = MathF.Max( Fov - 20, 20 ),
				OuterConeAngle = Fov,
				Brightness = 10,
				Color = Color,
				Rotation = Rotation.Identity,
				LightCookie = Texture.Load( "materials/effects/lightcookie.vtex" )
			};

			lamp.SetModel( Model );
			lamp.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
			lamp.Position = tr.EndPosition + -lamp.CollisionBounds.Center +
			                tr.Normal * lamp.CollisionBounds.Size * 0.5f;
			lamp.Rotation = Rotation.FromYaw( Owner.EyeRotation.Yaw() );
			UndoHandler.Register( Owner, lamp );
		}
	}

	public override void ReadSettings( BinaryReader streamReader )
	{
		Color = Color.Read( streamReader );
		Range = streamReader.ReadSingle();
		Fov = streamReader.ReadSingle();
	}

	private void UpdateSettings()
	{
		using SettingsWriter writer = new();
		Color.Write( writer );
		Range.Write( writer );
		Fov.Write( writer );
	}

	public override Panel MakeSettingsPanel()
	{
		SettingsPanel sPanel = new();
		sPanel.AddChild( new Title( "Lamp Color" ) );

		var cPicker = sPanel.Add.ColorPicker( clr =>
		{
			Color = clr;

			UpdateSettings();
		} );

		var rangeSlider = sPanel.Add.SliderLabeled( "Range", 0, 1024, 1 );
		rangeSlider.Value = Range;

		rangeSlider.OnFinalValue = rangeValue =>
		{
			Range = rangeValue;
			UpdateSettings();
		};

		var fovSlider = sPanel.Add.SliderLabeled( "FOV", 1, 90, 1 );
		fovSlider.Value = Fov;

		fovSlider.OnFinalValue = fovValue =>
		{
			Fov = fovValue;
			UpdateSettings();
		};


		return sPanel;
	}
}
