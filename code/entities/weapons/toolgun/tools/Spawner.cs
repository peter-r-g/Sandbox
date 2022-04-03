using Sandbox;
using SandboxGame.Entities;
using SandboxGame.UI;

namespace SandboxGame.Tools;

[Library( Constants.Tool.Spawner, Group = "construction" )]
public class SpawnerTool : BaseTool
{
	[ConVar.ClientDataAttribute( "spawner_model" )]
	public static string Model { get; set; }

	[ConVar.ClientDataAttribute( "spawner_entity" )]
	public static string EntityClass { get; set; }

	public override void Simulate()
	{
		if ( Host.IsClient )
		{
			if ( !Input.Pressed( InputButton.Reload ) )
			{
				return;
			}

			var tr = Trace.Ray( Owner.EyePosition, Owner.EyePosition + Owner.EyeRotation.Forward * 20000 )
				.Ignore( Owner )
				.Run();

			switch ( tr.Entity )
			{
				case Prop prop when !string.IsNullOrEmpty( prop.GetModelName() ):
					Model = prop.GetModelName();
					EntityClass = "";
					break;
				case ModelEntity entity when !string.IsNullOrEmpty( entity.GetModelName() ):
					Model = "";
					EntityClass = entity.ClassInfo.Name;
					break;
			}

			return;
		}

		if ( !Host.IsServer )
		{
			return;
		}

		if ( !Input.Pressed( InputButton.Attack1 ) )
		{
			return;
		}

		var model = Owner.Client.GetClientData<string>( "spawner_model", "" );
		if ( !string.IsNullOrWhiteSpace( model ) )
		{
			_ = SandboxGame.SpawnModel( Owner.Client, model );
		}

		var entityClass = Owner.Client.GetClientData<string>( "spawner_entity", "" );
		if ( !string.IsNullOrWhiteSpace( entityClass ) )
		{
			SandboxGame.SpawnEntity( Owner.Client, entityClass );
		}
	}

	[PropContextAction( ActionName = "#propaction_spawner" )]
	public static void SpawnWithToolgunPropAction( string modelName, bool sandworks )
	{
		Model = sandworks ? modelName : $"models/{modelName}";
		EntityClass = "";
		SandboxPlayer.EquipToolgunWithTool( Constants.Tool.Spawner );
	}

	[EntityContextAction( ActionName = "#entityaction_spawner" )]
	public static void SpawnWithToolgunEntityAction( string entityClass )
	{
		Model = "";
		EntityClass = entityClass;
		SandboxPlayer.EquipToolgunWithTool( Constants.Tool.Spawner );
	}
}
