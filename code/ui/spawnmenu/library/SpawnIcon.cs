using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using SandboxGame.AssetManager;

namespace SandboxGame.UI;

[UseTemplate]
public class SpawnIcon : Panel
{
	public SpawnIcon( string iconName )
	{
		IconText = iconName;
	}

	public Panel InnerPanel { get; set; }
	public Panel IconPanel { get; set; }
	public string IconText { get; set; }

	public SpawnIcon WithIcon( Texture icon )
	{
		IconPanel.Style.BackgroundImage = icon;
		return this;
	}

	public SpawnIcon WithIcon( string iconPath )
	{
		WithIcon( Texture.Load( FileSystem.Mounted, iconPath ) );
		return this;
	}

	public SpawnIcon WithCallback( Action<bool> clickCallback )
	{
		InnerPanel.AddEventListener( "onrightclick", () => clickCallback.Invoke( false ) );
		InnerPanel.AddEventListener( "onclick", () => clickCallback.Invoke( true ) );
		return this;
	}

	public SpawnIcon WithRenderedIcon( string modelPath )
	{
		var renderModel = Assets.Get<Model>( modelPath );

		var maxs = renderModel.RenderBounds.Maxs;
		var maxDist = Vector3.DistanceBetween( Vector3.Zero, maxs );

		Vector3 camNormal = new(.6f, .7f, .4f);
		var camPos = camNormal * maxDist + Vector3.Up * (maxs.z / 2);
		var camRot = Rotation.LookAt( -camNormal );


		var scene = new SceneWorld();
		new SceneModel( scene, modelPath, Transform.Zero );

		var scenePanel = IconPanel.Add.ScenePanel( scene, camPos, camRot, 90, "renderedCam" );
		scenePanel.AmbientColor = Color.White * 1;
		scenePanel.RenderOnce = true;


		IconPanel.AddClass( "noicon" );
		return this;
	}
}
