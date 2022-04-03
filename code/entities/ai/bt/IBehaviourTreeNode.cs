using System;

namespace SandboxGame.Vendor.FluentBehaviourTree;

/// <summary>
///     Interface for behaviour tree nodes.
/// </summary>
public interface IBehaviourTreeNode<in T>
{
	/// <summary>
	///     Name of the node.
	/// </summary>
	string Name { get; }
	
	/// <summary>
	///     Update the time of the behaviour tree.
	/// </summary>
	BehaviourTreeStatus Tick( T data );
}

public interface IBehaviourTreeNode : IBehaviourTreeNode<ValueType> {}
