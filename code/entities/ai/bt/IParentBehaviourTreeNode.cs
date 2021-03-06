using System;

namespace SandboxGame.Vendor.FluentBehaviourTree;

/// <summary>
///     Interface for behaviour tree nodes.
/// </summary>
public interface IParentBehaviourTreeNode<T> : IBehaviourTreeNode<T>
{
	/// <summary>
	///     Add a child to the parent node.
	/// </summary>
	void AddChild( IBehaviourTreeNode<T> child );
}

public interface IParentBehaviourTreeNode : IParentBehaviourTreeNode<ValueType> {}
