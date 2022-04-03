using Sandbox;

namespace SandboxGame.AssetManager;

public partial class LoadedMesh : ModelEntity
{
	[Net] public string ModelPath { get; set; }

	private string LastModel { get; set; }

	[Event.Tick]
	public void Tick()
	{
		if ( LastModel == ModelPath || !IsValid )
		{
			return;
		}

		LastModel = ModelPath;
		Model = Assets.Get<Model>( ModelPath );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
	}
}
