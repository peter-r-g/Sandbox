using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using SandboxGame.Entities;
using SandboxGame.Weapons;

namespace SandboxGame.Tools;

public class BaseTool : BaseNetworkable
{
	public ToolGun Parent { get; set; }
	public Player Owner { get; set; }

	protected virtual float MaxTraceDistance => 10000.0f;
	
	internal List<PreviewEntity> Previews;

	public virtual Panel MakeSettingsPanel()
	{
		return null;
	}

	public virtual void ReadSettings( BinaryReader streamReader )
	{
	}

	public virtual void Activate()
	{
		CreatePreviews();
	}

	public virtual void Deactivate()
	{
		DeletePreviews();
	}

	public virtual void Simulate()
	{
	}

	public virtual void OnFrame()
	{
		UpdatePreviews();
	}

	public virtual void CreateHitEffects( Vector3 pos )
	{
		Parent?.CreateHitEffects( pos );
	}

	protected virtual bool IsPreviewTraceValid( TraceResult tr )
	{
		if ( !tr.Hit )
		{
			return false;
		}

		if ( !tr.Entity.IsValid() )
		{
			return false;
		}

		return true;
	}

	public virtual void CreatePreviews()
	{
		// Nothing
	}

	public virtual void DeletePreviews()
	{
		if ( Previews == null || Previews.Count == 0 )
		{
			return;
		}

		foreach ( var preview in Previews )
		{
			preview.Delete();
		}

		Previews.Clear();
	}


	public virtual bool TryCreatePreview( ref PreviewEntity ent, string model )
	{
		if ( !ent.IsValid() )
		{
			ent = new PreviewEntity();
			ent.SetModel( model );
		}

		if ( Previews == null )
		{
			Previews = new List<PreviewEntity>();
		}

		if ( !Previews.Contains( ent ) )
		{
			Previews.Add( ent );
		}

		return ent.IsValid();
	}
	
	private void UpdatePreviews()
	{
		if ( Previews == null || Previews.Count == 0 )
		{
			return;
		}

		if ( !Owner.IsValid() )
		{
			return;
		}

		var startPos = Owner.EyePosition;
		var dir = Owner.EyeRotation.Forward;

		var tr = Trace.Ray( startPos, startPos + dir * 10000.0f )
			.Ignore( Owner )
			.Run();

		foreach ( var preview in Previews )
		{
			if ( !preview.IsValid() )
			{
				continue;
			}

			if ( IsPreviewTraceValid( tr ) && preview.UpdateFromTrace( tr ) )
			{
				preview.RenderColor = preview.RenderColor.WithAlpha( 0.5f );
			}
			else
			{
				preview.RenderColor = preview.RenderColor.WithAlpha( 0.0f );
			}
		}
	}
	
	[ServerCmd]
	public static void ReplicateSettings( string rawStream )
	{
		var callerTool = (ToolGun)ConsoleSystem.Caller.Pawn.Children.FirstOrDefault( x => x is ToolGun );
		if ( callerTool == null )
		{
			return;
		}

		var streamBytes = Convert.FromBase64String( rawStream );
		BinaryReader bReader = new(new MemoryStream( streamBytes ));

		callerTool.CurrentTool.ReadSettings( bReader );
		bReader.Close();
	}
}

public class SettingsWriter : BinaryWriter
{
	public SettingsWriter() : base( new MemoryStream() )
	{
	}

	protected override void Dispose( bool disposing )
	{
		var streamData = (MemoryStream)BaseStream;
		var streamString = Convert.ToBase64String( streamData.ToArray() );
		BaseTool.ReplicateSettings( streamString );
		base.Dispose( disposing );
	}
}
