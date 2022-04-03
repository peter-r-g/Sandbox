using System;
using SandboxGame.Debug;

namespace SandboxGame.Vendor.FluentBehaviourTree;

/// <summary>
///     A behaviour tree leaf node for running an action.
/// </summary>
public class ActionNode<T> : IBehaviourTreeNode<T>
{
	/// <summary>
	///     Name of the node.
	/// </summary>
	public string Name { get; }
	
	/// <summary>
	///     Function to invoke for the action.
	/// </summary>
	private readonly Func<T, BehaviourTreeStatus> fn;
	
	/// <summary>
	///     Function to invoke before the action.
	/// </summary>
	private readonly Action<T> beforeFn;
	
	/// <summary>
	///     Function to invoke after the action.
	/// </summary>
	private readonly Action<T> afterFn;

	private bool started;

	public ActionNode( string name, Func<T, BehaviourTreeStatus> fn, Action<T> beforeFn = null, Action<T> afterFn = null )
	{
		Name = name;

		this.fn = fn ?? throw new ArgumentNullException();
		this.beforeFn = beforeFn;
		this.afterFn = afterFn;
	}

	public BehaviourTreeStatus Tick( T data )
	{
#if DEBUG
		using var a = Profile.Scope( $"ActionNode::{Name}" );
#endif
		if ( !started )
		{
			beforeFn?.Invoke( data );
			started = true;
		}
		
		var result = fn( data );

		if ( result != BehaviourTreeStatus.Running )
		{
			afterFn?.Invoke( data );
			started = false;
		}

		return result;
	}
}
