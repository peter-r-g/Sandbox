using System;
using SandboxGame.Debug;

namespace SandboxGame.Vendor.FluentBehaviourTree;

/// <summary>
///     Decorator node that inverts the success/failure of its child.
/// </summary>
public class InverterNode<T> : IParentBehaviourTreeNode<T>
{
	/// <summary>
	///     Name of the node.
	/// </summary>
	public string Name { get; }
	
	/// <summary>
	///     The child to be inverted.
	/// </summary>
	private IBehaviourTreeNode<T> childNode;

	public InverterNode( string name )
	{
		Name = name;
	}

	public BehaviourTreeStatus Tick( T data )
	{
#if DEBUG
		using var a = Profile.Scope( $"InverterNode::{Name}" );
#endif
		if ( childNode == null )
		{
			throw new Exception( "InverterNode must have a child node!" );
		}

		var result = childNode.Tick( data );
		return result switch
		{
			BehaviourTreeStatus.Failure => BehaviourTreeStatus.Success,
			BehaviourTreeStatus.Success => BehaviourTreeStatus.Failure,
			_ => result
		};
	}

	/// <summary>
	///     Add a child to the parent node.
	/// </summary>
	public void AddChild( IBehaviourTreeNode<T> child )
	{
		if ( childNode != null )
		{
			throw new Exception( "Can't add more than a single child to InverterNode!" );
		}

		childNode = child;
	}
}
