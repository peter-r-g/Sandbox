using Sandbox;

namespace SandboxGame;

public static class ClothingOutfits
{
	private static void CreateModel( ModelEntity entity, string model )
	{
		if ( entity.GetModelName() != "models/citizen/citizen.vmdl" )
		{
			return;
		}
		
		var clothingEnt = new ModelEntity( model, entity );
		clothingEnt.Tags.Add( "clothes" );
	}
	
	public static void CivilianMale( ModelEntity entity )
	{
		CreateModel( entity, "models/citizen_clothes/hair/hair_malestyle02.vmdl" );
		CreateModel( entity, "models/citizen_clothes/jacket/jacket.red.vmdl" );
		CreateModel( entity, "models/citizen_clothes/trousers/trousers.jeans.vmdl" );
		CreateModel( entity, "models/citizen_clothes/shoes/trainers.vmdl" );
	}

	public static void CivilianFemale( ModelEntity entity )
	{
		CreateModel( entity, "models/citizen_clothes/hair/hair_femalebun.brown.vmdl" );
		CreateModel( entity, "models/citizen_clothes/dress/dress.kneelength.vmdl" );
		CreateModel( entity, "models/citizen_clothes/shoes/trainers.vmdl" );
	}
	
	public static void Police( ModelEntity entity )
	{
		CreateModel( entity, "models/citizen_clothes/hat/hat_uniform.police.vmdl" );
		CreateModel( entity, "models/citizen_clothes/shirt/shirt_longsleeve.police.vmdl" );
		CreateModel( entity, "models/citizen_clothes/trousers/trousers.police.vmdl" );
		CreateModel( entity, "models/citizen_clothes/shoes/shoes.police.vmdl" );
		CreateModel( entity, "models/citizen_clothes/vest/tactical_vest/models/tactical_vest.vmdl" );
	}
}
