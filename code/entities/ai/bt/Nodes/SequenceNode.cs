using System.Collections.Generic;
using System.Linq;
using SandboxGame.Debug;

namespace SandboxGame.Vendor.FluentBehaviourTree;

/// <summary>
///     Runs child nodes in sequence, until one fails.
/// </summary>
public class SequenceNode<T> : IParentBehaviourTreeNode<T>
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

	public SequenceNode( string name )
	{
		Name = name;
	}

	public BehaviourTreeStatus Tick( T data )
	{
#if DEBUG
		using var a = Profile.Scope( $"SequenceNode::{Name}" );
#endif
		return builtChildren.Select( child => child.Tick( data ) )
			.FirstOrDefault( childStatus => childStatus != BehaviourTreeStatus.Success );
	}

	/// <summary>
	///     Add a child to the sequence.
	/// </summary>
	public void AddChild( IBehaviourTreeNode<T> child )
	{
		children.Add( child );
		builtChildren = children.ToArray();
	}
}
