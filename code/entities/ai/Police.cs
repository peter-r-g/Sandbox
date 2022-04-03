using System.Linq;
using Sandbox;
using SandboxGame.Entities.AI;
using SandboxGame.Vendor.FluentBehaviourTree;
using SandboxGame.Weapons;

namespace SandboxGame.Entities;

[Library( Constants.Entity.Police, Spawnable = true )]
public class Police : CitizenAi
{
	private const int AttackDistanceThreshold = 500;

	public override float MaxHealth => 250;
	public override bool DropWeapon => true;
	
	public virtual bool HasTarget =>
		Target is not null &&
		Target.LifeState != LifeState.Dead &&
		Target.Position.Distance( Position ) <= AttackDistanceThreshold;

	protected Entity Target;

	public override void Spawn()
	{
		base.Spawn();
		
		Speed = Rand.Float( 100, 250 );
		
		Inventory.Add( new Pistol() );
		Inventory.Add( new Smg(), true );

		ClothingOutfits.Police( this );

		var attackTree = new BehaviourTreeBuilder<IAiContext>().Sequence( "Attack" )
			.Condition( "HasWeapon", _ => ActiveChild is Weapon )
			.Parallel( "AttackLogic", 3, 3 )
				.Selector( "SwitchOrReload" )
					.Sequence( "SwitchWeapon" )
						.Condition( "CanSwitch", _ =>
						{
							var nextWep = Inventory.GetNext();
							if ( nextWep is Weapon {ClipSize: 0} )
								return false;
							
							return Inventory.Count() > 1 && (ActiveChild as Weapon).ClipSize == 0;
						} )
						.Do( "DoSwitch", _ =>
						{
							ActiveChild = Inventory.GetNext();
							return BehaviourTreeStatus.Success;
						} )
					.End()
					.Sequence( "Reload" )
						.Condition( "CanReload", _ => ActiveChild is Weapon wep && (wep.ClipSize <= 0 || !HasTarget) )
						.Splice( CommonTrees.WeaponTrees.Reload )
					.End()
				.End()
				.Selector( "ValidateTarget" )
					.Condition( "HasTarget", _ => HasTarget )
					.Do( "FindTarget", _ =>
					{
						var foundTarget = FindInSphere( Position, AttackDistanceThreshold )
							.OfType<Player>().FirstOrDefault();

						Target = foundTarget;
						return Target is null ? BehaviourTreeStatus.Failure : BehaviourTreeStatus.Success;
					} )
				.End()
				.Sequence( "AttackTarget" )
					.Condition( "CanAttack", _ => HasTarget )
					.Do( "LookAtTarget", _ =>
					{
						EyeRotation = Rotation.LookAt( (Target.Position - Position).Normal, Vector3.Up );
						return BehaviourTreeStatus.Success;
					} )
					.Splice( CommonTrees.WeaponTrees.PrimaryAttack )
				.End()
			.End().End().Build();

		BehaviourTree = new BehaviourTreeBuilder<IAiContext>().Parallel( "All", 2, 2 )
				.Splice( attackTree )
				.Splice( CommonTrees.Movement.Wander( 5, 100, 1000 ) )
			.End().Build();
	}
}
