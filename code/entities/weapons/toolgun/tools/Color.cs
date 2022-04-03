using System.IO;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using SandboxGame.UI;

namespace SandboxGame.Tools;

[Library( Constants.Tool.Color, Group = "construction" )]
public class ColorTool : BaseTool
{
	public Color Color = Color.White;

	public override void Simulate()
	{
		if ( !Host.IsServer )
		{
			return;
		}

		using ( Prediction.Off() )
		{
			var startPos = Owner.EyePosition;
			var dir = Owner.EyeRotation.Forward;

			if ( !Input.Pressed( InputButton.Attack1 ) )
			{
				return;
			}

			var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
				.Ignore( Owner )
				.UseHitboxes()
				.HitLayer( CollisionLayer.Debris )
				.Run();

			if ( !tr.Hit || !tr.Entity.IsValid() )
			{
				return;
			}

			if ( tr.Entity is not ModelEntity modelEnt )
			{
				return;
			}

			modelEnt.RenderColor = Color;
			CreateHitEffects( tr.EndPosition );
		}
	}

	public override void ReadSettings( BinaryReader streamReader )
	{
		Color = Color.Read( streamReader );
	}

	private void UpdateSettings()
	{
		using SettingsWriter writer = new();
		Color.Write( writer );
	}

	public override Panel MakeSettingsPanel()
	{
		SettingsPanel sPanel = new();
		sPanel.AddChild( new Title( "Color" ) );

		var cPicker = sPanel.Add.ColorPicker( clr =>
		{
			Color = clr;

			UpdateSettings();
		} );

		return sPanel;
	}
}
