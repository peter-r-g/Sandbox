using System.Collections.Generic;
using System.Linq;
using SandboxGame.Debug;

namespace SandboxGame.Vendor.FluentBehaviourTree;

/// <summary>
///     Runs children nodes in parallel.
/// </summary>
public class ParallelNode<T> : IParentBehaviourTreeNode<T>
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

	/// <summary>
	///     Number of child failures required to terminate with failure.
	/// </summary>
	private readonly int numRequiredToFail;

	/// <summary>
	///     Number of child successes require to terminate with success.
	/// </summary>
	private readonly int numRequiredToSucceed;

	public ParallelNode( string name, int numRequiredToFail, int numRequiredToSucceed )
	{
		Name = name;
		this.numRequiredToFail = numRequiredToFail;
		this.numRequiredToSucceed = numRequiredToSucceed;
	}

	public BehaviourTreeStatus Tick( T data )
	{
#if DEBUG
		using var a = Profile.Scope( $"ParallelNode::{Name}" );
#endif
		var numChildrenSuceeded = 0;
		var numChildrenFailed = 0;

		foreach ( var childStatus in builtChildren.Select( child => child.Tick( data ) ) )
		{
			switch ( childStatus )
			{
				case BehaviourTreeStatus.Success:
					++numChildrenSuceeded;
					break;
				case BehaviourTreeStatus.Failure:
					++numChildrenFailed;
					break;
			}
		}

		if ( numRequiredToSucceed > 0 && numChildrenSuceeded >= numRequiredToSucceed )
		{
			return BehaviourTreeStatus.Success;
		}

		if ( numRequiredToFail > 0 && numChildrenFailed >= numRequiredToFail )
		{
			return BehaviourTreeStatus.Failure;
		}

		return BehaviourTreeStatus.Running;
	}

	public void AddChild( IBehaviourTreeNode<T> child )
	{
		children.Add( child );
		builtChildren = children.ToArray();
	}
}
