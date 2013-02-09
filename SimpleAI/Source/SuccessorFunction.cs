using System;

namespace SimpleAI.Framework
{
	/// <summary>
	/// A successor function takes a state and generates all possible actions that will produce a successor state.
	/// </summary>
	/// <remarks>
	/// The successor function is broken down into two parts: partitioning and expansion.
	/// First, the searcher calls Partition with the state to be partitioned and player to be evaluated.
	/// In this stage, the implementor should figure out how to partition the state, and call the partitioner once per partition with the partition data.
	/// The second stage is expansion.
	/// In this stage, the searcher will concurrently call Expand once per partition.  As a result, be careful of thread safety.
	/// From here, it is up to the implementor to generate all successor actions for the partition it receives.
	/// Make one call to the expander per generated action.
	/// If it returns false, the searcher is out of memory and Expand can stop generating actions (but that is not a requirement).
	/// </remarks>
	public interface SuccessorFunction <S, A, F, P> where S : State<A>
	{
		/// <summary>
		/// Partition the specified state for the given player into appropriately sized partitions.
		/// </summary>
		/// <param name='state'>
		/// The state to partition.
		/// </param>
		/// <param name='player'>
		/// The player to partition the state for.
		/// </param>
		/// <param name='partitioner'>
		/// The delegate to call one per partition.
		/// </param>
		void Partition (S state, P player, Action<F> partitioner);
		/// <summary>
		/// Given the state, and partition information, create all possible successor actions for that player.
		/// </summary>
		/// <param name='state'>
		/// The state to find successor actions for.
		/// </param>
		/// <param name='player'>
		/// The player who will perform the actions.
		/// </param>
		/// <param name='partition'>
		/// The partition data that was generated in the Partition method.
		/// Partitions should not overlap.
		/// </param>
		/// <param name='expander'>
		/// The delegate to call once per generated action.
		/// If the delegate returns false, the Expander can stop generating actions as the searcher is out of memory.
		/// </param>
		void Expand (S state, P player, F partition, Func<A, bool> expander);
	}
}
