using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;
using SandboxGame.Debug;
using SandboxGame.Entities;
using SandboxGame.Entities.AI;
using SandboxGame.Tools;
using SandboxGame.UI;
using SandboxGame.Weapons;

namespace SandboxGame;

public partial class SandboxGame : Game
{
	public static SandboxGame Instance => Current as SandboxGame;

	[Net] public Dictionary<long, int> UndoCount { get; set; }
	
	public SandboxGame()
	{
		Language.Initialize( Constants.BackupLanguage );
		if ( IsClient )
		{
			return;
		}

		_ = new SandboxHud();
	}

	public override void Simulate( Client cl )
	{
#if DEBUG
		using var a = Profile.Scope( "Game::Simulate::Base" );
#endif
		base.Simulate( cl );
		
#if DEBUG
		a?.Dispose();
		using var b = Profile.Scope( "Game::Simulate::Ai" );
#endif
		
		foreach ( var ai in All.OfType<CitizenAi>() )
			ai.Simulate( cl );
	}

	public override void FrameSimulate( Client cl )
	{
#if DEBUG
		using var a = Profile.Scope( "Game::FrameSimulate::Base" );
#endif
		base.FrameSimulate( cl );
		
#if DEBUG
		a?.Dispose();
		using var b = Profile.Scope( "Game::FrameSimulate::Ai" );
#endif
		
		foreach ( var ai in All.OfType<CitizenAi>() )
			ai.FrameSimulate( cl );
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );
		var player = new SandboxPlayer( cl );
		player.Respawn();

		cl.Pawn = player;
	}

	public override void DoPlayerNoclip( Client cl )
	{
		if ( cl.Pawn is not Player basePlayer )
		{
			return;
		}

		basePlayer.DevController = basePlayer.DevController is NoclipController ? null : new NoclipController();
	}

	[Event.Hotload]
	private void HotloadLanguage()
	{
		Language.Initialize( Constants.BackupLanguage );
	}

	public static bool CleanupFilter( string className, Entity ent )
	{
		return ent is not SandboxHud && DefaultCleanupFilter( className, ent );
	}

	private static async Task<string> SpawnPackageModel( string packageName, Vector3 pos, Rotation rotation,
		Entity source )
	{
		var package = await Package.Fetch( packageName, false );
		if ( package is not {PackageType: Package.Type.Model} || package.Revision == null )
		{
			// spawn error particles
			return null;
		}

		if ( !source.IsValid )
		{
			return null; // source entity died or disconnected or something
		}

		var model = package.GetMeta( "PrimaryAsset", "models/dev/error.vmdl" );

		// downloads if not downloads, mounts if not mounted
		await package.MountAsync();

		return model;
	}

	public static async Task SpawnModel( Client cl, string model, Vector3 pos )
	{
		var owner = cl.Pawn;
		var ownerTool = (ToolGun)owner.Children.FirstOrDefault( x => x is ToolGun );

		var rotation = Rotation.From( new Angles( 0, owner.EyeRotation.Angles().yaw, 0 ) ) *
		               Rotation.FromAxis( Vector3.Up, 180 );

		//
		// Does this look like a package?
		//
		if ( model.Count( x => x == '.' ) == 1 &&
		     !model.EndsWith( ".vmdl", StringComparison.OrdinalIgnoreCase ) &&
		     !model.EndsWith( ".vmdl_c", StringComparison.OrdinalIgnoreCase ) )
		{
			Notifications.Send( To.Single( cl ), "sworksSpawn", "Spawning S&Works items may take some time" );
			model = await SpawnPackageModel( model, pos, rotation, owner );
			if ( model == null )
			{
				return;
			}
		}

		var ent = new Prop {Rotation = rotation};
		ent.SetModel( model );
		ent.Position = pos - Vector3.Up * ent.CollisionBounds.Mins.z;
		ent.Tags.Add( "prop", "spawned" );

		if ( model == "models/citizen/citizen.vmdl" && ownerTool?.CurrentTool is Dresser dresser )
		{
			dresser.ApplyClothing( ent );
		}

		UndoHandler.Register( owner, ent );
	}

	public static void SpawnEntity( Client cl, string entityClass, Vector3 pos )
	{
		var owner = cl.Pawn;
		var attribute = Library.GetAttribute( entityClass );
		if ( attribute is not {Spawnable: true} )
		{
			return;
		}

		var ent = Library.Create<Entity>( entityClass );
		if ( owner is Player player && ent is BaseCarriable && player.Inventory != null )
		{
			if ( player.Inventory.Add( ent, true ) )
			{
				return;
			}
		}

		ent.Position = pos;
		ent.Rotation = Rotation.From( new Angles( 0, owner.EyeRotation.Angles().yaw, 0 ) );
		ent.Tags.Add( "entity", "spawned" );
		UndoHandler.Register( owner, ent );
	}
	
	[ServerCmd]
	public static async Task SpawnModelCmd( string model )
	{
		if ( ConsoleSystem.Caller is null )
		{
			Log.Warning( Language.GetPhrase( "console_cant_use" ) );
			return;
		}

		var owner = ConsoleSystem.Caller.Pawn;
		var tr = Trace.Ray( owner.EyePosition, owner.EyePosition + owner.EyeRotation.Forward * 500 )
			.UseHitboxes()
			.Ignore( owner )
			.Run();

		await SpawnModel( ConsoleSystem.Caller, model, tr.EndPosition );
	}

	[ServerCmd]
	public static void SpawnEntityCmd( string entityClass )
	{
		if ( ConsoleSystem.Caller is null )
		{
			Log.Warning( Language.GetPhrase( "console_cant_use" ) );
			return;
		}
		
		var owner = ConsoleSystem.Caller.Pawn;
		var tr = Trace.Ray( owner.EyePosition, owner.EyePosition + owner.EyeRotation.Forward * 500 )
			.UseHitboxes()
			.Ignore( owner )
			.Run();

		SpawnEntity( ConsoleSystem.Caller, entityClass, tr.EndPosition );
	}
}
