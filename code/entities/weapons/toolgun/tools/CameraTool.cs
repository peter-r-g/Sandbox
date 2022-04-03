using Sandbox;
using SandboxGame.UI;

namespace SandboxGame.Tools;

[Library( Constants.Tool.Camera )]
public class CameraTool : BaseTool
{
	public override void Activate()
	{
		base.Activate();
		if ( Host.IsServer )
		{
			return;
		}

		Notifications.Send( "camwarning1", "#camera1", 3 );
		Notifications.Send( "camwarning2", "#camera2", 3 );
	}

	public override void Simulate()
	{
		if ( Parent?.ViewModelEntity is ModelEntity ent )
		{
			ent.EnableDrawing = false;
		}

		base.Simulate();
	}

	public override void Deactivate()
	{
		if ( Parent?.ViewModelEntity is ModelEntity ent )
		{
			ent.EnableDrawing = true;
		}

		base.Deactivate();
	}
}
