using Sandbox;

namespace SandboxGame.Entities;

public class Carriable : BaseCarriable, IUse
{
	public bool OnUse( Entity user )
	{
		return false;
	}

	public virtual bool IsUsable( Entity user )
	{
		return Owner == null;
	}
	
	public virtual void SimulateAnimator( AnimEntity animEntity )
	{
		
	}

	public override void CreateViewModel()
	{
		Host.AssertClient();

		if ( string.IsNullOrEmpty( ViewModelPath ) )
		{
			return;
		}

		ViewModelEntity = new ViewModel {Position = Position, Owner = Owner, EnableViewmodelRendering = true};

		ViewModelEntity.SetModel( ViewModelPath );
	}
}
