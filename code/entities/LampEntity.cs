using Sandbox;

namespace SandboxGame.Entities;

[Library( Constants.Entity.Lamp, Spawnable = true )]
public class LampEntity : SpotLightEntity, IUse
{
	public bool IsUsable( Entity user )
	{
		return true;
	}

	public bool OnUse( Entity user )
	{
		Enabled = !Enabled;

		PlaySound( Enabled ? "flashlight-on" : "flashlight-off" );

		return false;
	}

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/torch/torch.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
	}

	public void Remove()
	{
		PhysicsGroup.Sleeping = false;
		Delete();
	}
}
