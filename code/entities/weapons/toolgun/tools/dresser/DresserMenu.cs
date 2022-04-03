using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using SandboxGame.UI;

namespace SandboxGame.Tools;

[UseTemplate]
public class DresserMenu : SettingsPanel
{
	public static List<string> Clothing = new()
	{
		"models/citizen_clothes/hat/hat_hardhat.vmdl",
		"models/citizen_clothes/hat/hat_woolly.vmdl",
		"models/citizen_clothes/hat/hat_securityhelmet.vmdl",
		"models/citizen_clothes/hair/hair_malestyle02.vmdl",
		"models/citizen_clothes/hair/hair_femalebun.black.vmdl",
		"models/citizen_clothes/hair/hair_looseblonde/hair_looseblonde.vmdl",
		"models/citizen_clothes/hat/hat_beret.red.vmdl",
		"models/citizen_clothes/hat/hat.tophat.vmdl",
		"models/citizen_clothes/hat/hat_beret.black.vmdl",
		"models/citizen_clothes/hat/hat_cap.vmdl",
		"models/citizen_clothes/hat/hat_leathercap.vmdl",
		"models/citizen_clothes/hat/hat_leathercapnobadge.vmdl",
		"models/citizen_clothes/hat/hat_securityhelmetnostrap.vmdl",
		"models/citizen_clothes/hat/hat_service.vmdl",
		"models/citizen_clothes/hat/hat_uniform.police.vmdl",
		"models/citizen_clothes/hat/hat_woollybobble.vmdl",
		"models/citizen_clothes/jacket/suitjacket/suitjacket.vmdl",
		"models/citizen_clothes/jacket/labcoat.vmdl",
		"models/citizen_clothes/jacket/jacket.red.vmdl",
		"models/citizen_clothes/jacket/jacket.tuxedo.vmdl",
		"models/citizen_clothes/jacket/jacket_heavy.vmdl",
		"models/citizen_clothes/dress/dress.kneelength.vmdl",
		"models/citizen_clothes/trousers/trousers.jeans.vmdl",
		"models/citizen_clothes/trousers/trousers.lab.vmdl",
		"models/citizen_clothes/trousers/trousers.police.vmdl",
		"models/citizen_clothes/trousers/trousers.smart.vmdl",
		"models/citizen_clothes/trousers/trousers.smarttan.vmdl",
		"models/citizen_clothes/trousers/trousers_tracksuitblue.vmdl",
		"models/citizen_clothes/trousers/trousers_tracksuit.vmdl",
		"models/citizen_clothes/trousers/SmartTrousers/smarttrousers.vmdl",
		"models/citizen_clothes/shoes/shorts.cargo.vmdl",
		"models/citizen_clothes/shoes/trainers.vmdl",
		"models/citizen_clothes/shoes/shoes.workboots.vmdl",
		"models/citizen_clothes/shoes/SmartShoes/smartshoes.vmdl"
	};

	private readonly List<SceneModel> Models = new();
	private SceneModel Preview;
	private SceneWorld SceneWorld;
	private SceneWorld World = new();

	public DresserMenu()
	{
		MakePreview();

		foreach ( var file in Clothing )
		{
			ClothingSelection.AddEntry( file, () => { } );
		}
	}

	private ModelSelector ClothingSelection { get; set; }
	private SliderLabeled SkinSlider { get; set; }
	private Panel RenderContainer { get; set; }

	private void MakePreview()
	{
		SceneWorld = new SceneWorld();

		Preview = new SceneModel( SceneWorld, Model.Load( "models/citizen/citizen.vmdl" ), Transform.Zero );
		Preview.SetAnimParameter( "idle_states", 0 );

		var scenePanel = RenderContainer.Add.ScenePanel( SceneWorld, new Vector3( 125, 0, 34 ), Rotation.FromYaw( 180 ),
			40, "renderview" );
		scenePanel.AmbientColor = Color.White * 1;

		var isHolding = false;
		scenePanel.AddEventListener( "onmouseup", () => isHolding = false );
		scenePanel.AddEventListener( "onmousedown", () => isHolding = true );

		float rot = 180;
		float yAdjust = 0;
		scenePanel.AddEventListener( "onmousemove", () =>
		{
			var cRot = Rotation.FromYaw( rot );

			scenePanel.CameraPosition = cRot.Forward * -125 + Vector3.Up * (32 + yAdjust);
			scenePanel.CameraRotation = cRot;

			if ( !isHolding )
			{
				return;
			}

			yAdjust = Math.Clamp( yAdjust + Mouse.Delta.y * .5f, -32, 32 );
			rot = (rot - Mouse.Delta.x * 2f) % 360f;
		} );
	}

	public override void Tick()
	{
		base.Tick();

		var toDelete = Models.Where( x => !ClothingSelection.Models.Contains( x.Model.Name ) );
		Preview.SetMaterialGroup( $"Skin0{SkinSlider.Value}" );

		foreach ( var model in toDelete.ToList() )
		{
			Models.Remove( model );
			model.Delete();
		}

		var currentClothes = Models.Select( x => x.Model.Name );
		var toMake = ClothingSelection.Models.Except( currentClothes );

		foreach ( var model in toMake )
		{
			var newObj = new SceneModel( SceneWorld, model, Preview.Transform );
			Preview.AddChild( "clothing", newObj );
			Models.Add( newObj );
		}

		Preview.Update( Time.Delta );
		foreach ( var mdl in Models )
		{
			mdl.Update( Time.Delta );
		}
	}

	public void UpdateSettings()
	{
		var currentClothes = Models.Select( x => x.Model.Name ).ToList();

		using ( SettingsWriter writer = new() )
		{
			SkinSlider.Value.Write( writer );
			currentClothes.Count.Write( writer );
			foreach ( var clothing in currentClothes )
			{
				clothing.Write( writer );
			}
		}
	}
}
