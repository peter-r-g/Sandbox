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

			var tr = Trace.Ray( Owner.EyePosition, Owner.EyePosition + Owner.EyeRotation.Forward * MaxTraceDistance )
				.Ignore( Owner )
				.Run();

			switch ( tr.Entity )
			{
				case Prop prop when !string.IsNullOrEmpty( prop.GetModelName() ):
					Model = prop.GetModelName();
					EntityClass = "";
					CreateHitEffects( tr.EndPosition );
					break;
				case ModelEntity entity when !string.IsNullOrEmpty( entity.GetModelName() ):
					Model = "";
					EntityClass = entity.ClassInfo.Name;
					CreateHitEffects( tr.EndPosition );
					break;
			}

			return;
		}

		if ( !Host.IsServer || !Input.Pressed( InputButton.Attack1 ) )
		{
			return;
		}

		var spawnTr = Trace.Ray( Owner.EyePosition, Owner.EyePosition + Owner.EyeRotation.Forward * MaxTraceDistance )
			.Ignore( Owner )
			.Run();

		var model = Owner.Client.GetClientData<string>( "spawner_model", "" );
		if ( !string.IsNullOrWhiteSpace( model ) )
		{
			_ = SandboxGame.SpawnModel( Owner.Client, model, spawnTr.EndPosition );
			CreateHitEffects( spawnTr.EndPosition );
		}

		var entityClass = Owner.Client.GetClientData<string>( "spawner_entity", "" );
		if ( !string.IsNullOrWhiteSpace( entityClass ) )
		{
			SandboxGame.SpawnEntity( Owner.Client, entityClass, spawnTr.EndPosition );
			CreateHitEffects( spawnTr.EndPosition );
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
