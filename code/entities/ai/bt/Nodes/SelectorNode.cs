using System.Collections.Generic;
using SandboxGame.Debug;

namespace SandboxGame.Vendor.FluentBehaviourTree;

/// <summary>
///     Selects the first node that succeeds. Tries successive nodes until it finds one that doesn't fail.
/// </summary>
public class SelectorNode<T> : IParentBehaviourTreeNode<T>
{
	/// <summary>
	///     Name of the node.
	/// </summary>
	public string Name { get; }
	
	/// <summary>
	///     List of child nodes.
	/// </summary>
	private readonly List<IBehaviourTreeNode<T>> children = new();
	
	/// <summary>
	///		The compiled list of child nodes.
	/// </summary>
	private IBehaviourTreeNode<T>[] builtChildren;

	public SelectorNode( string name )
	{
		Name = name;
	}

	public BehaviourTreeStatus Tick( T data )
	{
#if DEBUG
		using var a = Profile.Scope( $"SelectorNode::{Name}" );
#endif
		foreach ( var child in builtChildren )
		{
			var childStatus = child.Tick( data );
			if ( childStatus != BehaviourTreeStatus.Failure )
			{
				return childStatus;
			}
		}

		return BehaviourTreeStatus.Failure;
	}

	/// <summary>
	///     Add a child node to the selector.
	/// </summary>
	public void AddChild( IBehaviourTreeNode<T> child )
	{
		children.Add( child );
		builtChildren = children.ToArray();
	}
}
