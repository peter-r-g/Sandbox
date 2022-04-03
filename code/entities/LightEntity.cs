using Sandbox;

namespace SandboxGame.Entities;

[Library( Constants.Entity.Light, Spawnable = true )]
public class LightEntity : PointLightEntity, IUse
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

		SetModel( "models/light/light_tubular.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
	}

	public void Remove()
	{
		if ( PhysicsGroup.IsValid() )
		{
			PhysicsGroup.Sleeping = false;
		}

		Delete();
	}
}
