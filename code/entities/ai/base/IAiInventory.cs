using Sandbox;

namespace SandboxGame.Entities.AI;

public interface IAiInventory : IBaseInventory
{
	bool Contains( string entityClass );
	Entity Get( string entityClass );
	Entity GetNext( int skip = 0 );
}
