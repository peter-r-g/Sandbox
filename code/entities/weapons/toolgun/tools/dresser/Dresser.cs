using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace SandboxGame.Tools;

[Library( Constants.Tool.Dresser, Group = "construction" )]
public class Dresser : BaseTool
{
	public List<string> Models = new();
	public float Skin = 1;

	public override void Simulate()
	{
		if ( Host.IsClient )
		{
			return;
		}

		if ( !Input.Pressed( InputButton.Attack1 ) )
		{
			return;
		}

		var tr = Trace.Ray( Owner.EyePosition, Owner.EyePosition + Owner.EyeRotation.Forward * MaxTraceDistance )
			.Ignore( Owner )
			.UseHitboxes()
			.Run();

		if ( !tr.Hit || tr.Entity is Player || tr.Entity is not ModelEntity ent ||
		     ent.Model.Name != "models/citizen/citizen.vmdl" )
		{
			return;
		}

		CreateHitEffects( tr.EndPosition );
		ApplyClothing( ent );
	}

	public void ApplyClothing( ModelEntity ent )
	{
		ent.SetMaterialGroup( (int)Skin );

		foreach ( var child in ent.Children.ToList() )
		{
			if ( child.Tags.Has( "clothing" ) )
			{
				child.Delete();
			}
		}

		foreach ( var modelPath in Models )
		{
			ModelEntity modelEnt = new(modelPath, ent);
			modelEnt.Tags.Add( "clothing" );
		}
	}

	public override void ReadSettings( BinaryReader streamReader )
	{
		Skin = streamReader.ReadSingle();

		var clothingCount = streamReader.ReadInt32();
		Models.Clear();

		for ( var i = 0; i < clothingCount; i++ )
		{
			Models.Add( streamReader.ReadString() );
		}
	}

	public override Panel MakeSettingsPanel()
	{
		return new DresserMenu();
	}
}
