using Sandbox;

namespace SandboxGame.Entities;

public class EmptyBot : Bot
{
	public override void BuildInput( InputBuilder builder ) { }
	public override void Tick() { }

	[AdminCmd( Constants.Command.EmptyBot )]
	internal static void SpawnEmptyBot()
	{
		_ = new EmptyBot();
	}
}
