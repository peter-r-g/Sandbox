using Sandbox;
using SandboxGame.Entities;
using SandboxGame.Tools;

namespace SandboxGame.Weapons;

[Library( Constants.Weapon.Toolgun )]
public partial class ToolGun : Carriable
{
	[ConVar.ClientDataAttribute( Constants.Command.CurrentTool )]
	public static string UserToolCurrent { get; set; } = "tool_boxgun";

	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	[Net] [Predicted] public BaseTool CurrentTool { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
	}

	public override void Simulate( Client owner )
	{
		UpdateCurrentTool( owner );

		CurrentTool?.Simulate();
	}

	private void UpdateCurrentTool( Client owner )
	{
		var toolName = owner.GetClientData<string>( "tool_current", "tool_boxgun" );
		if ( toolName == null )
		{
			return;
		}

		// Already the right tool
		if ( CurrentTool != null && Library.GetAttribute( CurrentTool.GetType() ).Name == toolName )
		{
			return;
		}

		if ( CurrentTool != null )
		{
			CurrentTool?.Deactivate();
			CurrentTool = null;
		}

		CurrentTool = Library.Create<BaseTool>( toolName, false );

		if ( CurrentTool != null )
		{
			CurrentTool.Owner = owner.Pawn as Player;
			CurrentTool.Parent = this;
			CurrentTool.Activate();
		}
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		if ( CurrentTool is not null )
		{
			CurrentTool.Owner = Owner as Player;
			CurrentTool.Parent = this;
			CurrentTool.Activate();
		}
	}

	public override void ActiveEnd( Entity ent, bool dropped )
	{
		base.ActiveEnd( ent, dropped );

		CurrentTool?.Deactivate();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		CurrentTool?.Deactivate();
		CurrentTool = null;
	}

	public override void OnCarryDrop( Entity dropper )
	{
	}

	[Event.Frame]
	public void OnFrame()
	{
		if ( Owner is Player player && player.ActiveChild != this )
		{
			return;
		}

		CurrentTool?.OnFrame();
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 1 );
		anim.SetAnimParameter( "aim_body_weight", 1.0f );
		anim.SetAnimParameter( "holdtype_handedness", 1 );
	}
	
	public override void SimulateAnimator( AnimEntity animEntity )
	{
		animEntity.SetAnimParameter( "holdtype", 1 );
		animEntity.SetAnimParameter( "aim_body_weight", 1.0f );
		animEntity.SetAnimParameter( "holdtype_handedness", 1 );
	}
	
	[ClientRpc]
	public void CreateHitEffects( Vector3 hitPos )
	{
		var particle = Particles.Create( "particles/tool_hit.vpcf", hitPos );
		particle.SetPosition( 0, hitPos );
		particle.Destroy();

		PlaySound( "balloon_pop_cute" );
	}
}
