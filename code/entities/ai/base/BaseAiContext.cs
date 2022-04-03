using SandboxGame.Entities.AI;

namespace SandboxGame.Entities;

public class BaseAiContext : IAiContext
{
	public CitizenAi Entity { get; }

	public BaseAiContext( CitizenAi entity )
	{
		Entity = entity;
	}
}
