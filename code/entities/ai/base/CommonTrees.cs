using System.Collections.Generic;
using Sandbox;
using SandboxGame.Vendor.FluentBehaviourTree;
using SandboxGame.Weapons;

namespace SandboxGame.Entities.AI;

public static class CommonTrees
{
	public static class Movement
	{
		private static readonly Dictionary<string, IBehaviourTreeNode<IAiContext>> WanderCache = new();
		public static IBehaviourTreeNode<IAiContext> Wander( int idleToWanderTime, int wanderMinRadius, int wanderMaxRadius )
		{
			var valueKey = $"{idleToWanderTime}{wanderMinRadius}{wanderMaxRadius}";
			if ( WanderCache.TryGetValue( valueKey, out var item ) )
				return item;
		
			var wanderTree = new BehaviourTreeBuilder<IAiContext>().Sequence( "CommonTrees::Wander" )
				.Condition( "CommonTrees::MovementSupported", _ => NavMesh.IsLoaded )
				.Condition( "CommonTrees::CheckWander", context =>
				{
					var entity = context.Entity;
					return !(entity.TimeSinceReachedNavDestination < idleToWanderTime) && entity.Idle;
				} )
				.Do( "CommonTrees::Wander", context =>
				{
					var entity = context.Entity;
					entity.EyeRotation = entity.Rotation;
					return entity.NavMovementState == NavMovementState.Completed
						? BehaviourTreeStatus.Success
						: BehaviourTreeStatus.Running;
				}, context =>
				{
					var entity = context.Entity;
					var wanderTarget = NavMesh.GetPointWithinRadius( entity.Position, wanderMinRadius, wanderMaxRadius );
					entity.NavDestination = wanderTarget ?? entity.Position;
				} )
				.End().Build();

			WanderCache.Add( valueKey, wanderTree );
			return wanderTree;
		}
	}

	public static class WeaponTrees
	{
		public static readonly IBehaviourTreeNode<IAiContext> PrimaryAttack = new BehaviourTreeBuilder<IAiContext>()
			.Sequence( "CommonTrees::WeaponTrees::PrimaryAttack" )
			.Condition( "CommonTrees::WeaponTrees::CanPrimaryAttack",
				context => context.Entity.ActiveChild is Weapon wep && wep.CanPrimaryAttack( false ) )
			.Do( "CommonTrees::WeaponTrees::DoPrimaryAttack", context =>
			{
				(context.Entity.ActiveChild as Weapon)?.AttackPrimary();
				return BehaviourTreeStatus.Success;
			} )
			.End().Build();

		public static readonly IBehaviourTreeNode<IAiContext> SecondaryAttack = new BehaviourTreeBuilder<IAiContext>()
			.Sequence( "CommonTrees::WeaponTrees::SecondaryAttack" )
			.Condition( "CommonTrees::WeaponTrees::CanSecondaryAttack",
				context => context.Entity.ActiveChild is Weapon wep && wep.CanSecondaryAttack( false ) )
			.Do( "CommonTrees::WeaponTrees::DoSecondaryAttack", context =>
			{
				(context.Entity.ActiveChild as Weapon)?.AttackSecondary();
				return BehaviourTreeStatus.Success;
			} )
			.End().Build();

		public static readonly IBehaviourTreeNode<IAiContext> Reload = new BehaviourTreeBuilder<IAiContext>()
			.Sequence( "CommonTrees::WeaponTrees::Reload" )
			.Condition( "CommonTrees::WeaponTrees::CanReload",
				context => context.Entity.ActiveChild is Weapon wep && wep.CanReload( false ) )
			.Do( "CommonTrees::WeaponTrees::DoReload", context => (context.Entity.ActiveChild as Weapon).IsReloading
				? BehaviourTreeStatus.Running
				: BehaviourTreeStatus.Success, context => (context.Entity.ActiveChild as Weapon)?.Reload() )
			.End().Build();
	}
}
