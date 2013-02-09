// http://paulmoore.mit-license.org

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SimpleAI.Framework
{
	/// <summary>
	/// The main search class.
	/// </summary>
	/// <remarks>
	/// This class implements a minimax search with alpha-beta pruning.
	/// </remarks>
	public sealed class Search <S, A, V, F, P> : IComparer<A> where S : State<A>, MutableClone<S>
	{
		private readonly A[] actions;
		private readonly List<ManualResetEvent> resets;
		private readonly CloneCache<S> stateCache;
		private readonly CloneCache<EvaluationFunction<S, A, V, P>> evalCache;
		
		private P maxPlayer, minPlayer;
		private S state;
		private uint cutoffDepth, maxDepth;
		private bool isCutoff;
		
		/// <summary>
		/// Gets or sets the initial depth.
		/// The depth will automatically increase with each subsequent call to search.
		/// </summary>
		/// <value>
		/// The initial depth the search begins at.
		/// </value>
		public uint InitialDepth
		{
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the max explorations.
		/// If the successor function produces more actions than this value, they are sorted and truncated to this value.
		/// </summary>
		/// <value>
		/// The maximum amount of child actions to expand.
		/// </value>
		public uint MaxExplorations
		{
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the evaluation function.
		/// This object will be cloned during the search.
		/// </summary>
		/// <value>
		/// The evaluation function.
		/// </value>
		public EvaluationFunction<S, A, V, P> EvalFunc
		{
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the successor function.
		/// </summary>
		/// <value>
		/// The successor function.
		/// </value>
		public SuccessorFunction<S, A, F, P> SuccessorFunc
		{
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the cutoff test.
		/// </summary>
		/// <value>
		/// The cutoff test.
		/// </value>
		public CutoffTest<S, A> Cutoff
		{
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the action builder.
		/// </summary>
		/// <value>
		/// The action builder.
		/// </value>
		public ActionBuilder<A, V> Builder
		{
			get; set;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleAI.Framework.Search`5"/> class.
		/// </summary>
		/// <param name='maxActions'>
		/// The maximum amount of actions to have generated at once.
		/// This space will be stored up front and cannot be changed.
		/// </param>
		public Search (int maxActions)
		{
			int workers, ports;
			ThreadPool.GetAvailableThreads(out workers, out ports);
			resets = new List<ManualResetEvent>(workers);
			stateCache = new CloneCache<S>();
			evalCache = new CloneCache<EvaluationFunction<S, A, V, P>>();
			actions = new A[maxActions];
			InitialDepth = 1;
			MaxExplorations = uint.MaxValue;
		}
		
		/// <summary>
		/// Performs a search on the game tree for the best move.
		/// </summary>
		/// <returns>
		/// The best move the search could come up with.
		/// </returns>
		/// <param name='state'>
		/// The initial state to perform the search on.
		/// </param>
		/// <param name='maxPlayer'>
		/// The max player (the player making the move).
		/// </param>
		/// <param name='minPlayer'>
		/// The min player (the current player's opponent).
		/// </param>
		public A MinimaxDecision (S state, P maxPlayer, P minPlayer)
		{
			this.maxPlayer = maxPlayer;
			this.minPlayer = minPlayer;
			this.state = state;
			stateCache.Parent = state;
			evalCache.Parent = EvalFunc;
			cutoffDepth = InitialDepth;
			isCutoff = false;
			uint bestDepth = 0;
			A best = default(A);
			Cutoff.Begin();
			do {
				maxDepth = 0;
				A decision = MaxValue(default(A), Builder.NegInf, Builder.PosInf, 1, 0);
				// record new bests only if the search was not cutoff
				// this is to prevent false positives if the search was cutoff at a lower depth
				if (maxDepth > bestDepth && !isCutoff) {
					bestDepth = maxDepth;
					best = decision;
				}
				cutoffDepth++;
			} while (!isCutoff);
			Cutoff.End();
			// update the initial depth if we have reached a new fully explored depth
			InitialDepth = Math.Max(bestDepth - 1, InitialDepth);
			// reset all values so this search can be ran again
			Parallel.For(0, actions.Length, i => {
				actions[i] = default(A);
			});
			this.maxPlayer = default(P);
			this.minPlayer = default(P);
			this.state = default(S);
			stateCache.Clear();
			stateCache.Parent = null;
			evalCache.Clear();
			evalCache.Parent = null;
			return best;
		}
		
		private A MaxValue (A parent, V alpha, V beta, uint depth, int startIndex)
		{
			maxDepth = Math.Max(maxDepth, depth);
			if (Cutoff.Test(state)) {
				// search was cutoff, start ending the search
				isCutoff = true;
				return EndEarly(parent);
			}
			if (depth >= cutoffDepth) {
				// IDS cutoff
				return EndEarly(parent);
			}
			int endIndex = GenerateActions(maxPlayer, startIndex);
			if (endIndex < 0) {
				// out of memory
				isCutoff = true;
				return EndEarly(parent);
			}
			int i = startIndex;
			V v = Builder.NegInf;
			A best = default(A);
			while (i <= endIndex && !isCutoff) {
				A action = actions[i];
				// all cached states need to be updated with the current action
				UpdateAllStates(action);
				A minAction = MinValue(action, alpha, beta, depth + 1, endIndex + 1);
				V minValue = Builder.ExtractValue(minAction);
				// important to undo this action so a new one may be applied next iteration
				UndoAllStates(action);
				// pruning step
				if (Builder.Compare(v, minValue) < 0) {
					v = minValue;
					best = action;
				}
				if (Builder.Compare(v, beta) >= 0) {
					break;
				}
				if (Builder.Compare(v, alpha) > 0) {
					alpha = v;
				}
				i++;
			}
			return Builder.InsertValue(best, v);
		}
		
		private A MinValue (A parent, V alpha, V beta, uint depth, int startIndex)
		{
			maxDepth = Math.Max(maxDepth, depth);
			if (Cutoff.Test(state)) {
				// search was cutoff, start ending the search
				isCutoff = true;
				return EndEarly(parent);
			}
			if (depth >= cutoffDepth) {
				// IDS cutoff
				return EndEarly(parent);
			}
			int endIndex = GenerateActions(minPlayer, startIndex);
			if (endIndex < 0) {
				// out of memory
				isCutoff = true;
				return EndEarly(parent);
			}
			int i = startIndex;
			V v = Builder.PosInf;
			A best = default(A);
			while (i <= endIndex && !isCutoff) {
				A action = actions[i];
				// all cached states need to be updated with the current action
				UpdateAllStates(action);
				A maxAction = MaxValue(action, alpha, beta, depth + 1, endIndex + 1);
				V maxValue = Builder.ExtractValue(maxAction);
				// important to undo this action so a new one may be applied next iteration
				UndoAllStates(action);
				// pruning step
				if (Builder.Compare(v, maxValue) > 0) {
					v = maxValue;
					best = action;
				}
				if (Builder.Compare(v, alpha) <= 0) {
					break;
				}
				if (Builder.Compare(v, beta) < 0) {
					beta = v;
				}
				i++;
			}
			return Builder.InsertValue(best, v);
		}
		
		private int GenerateActions (P player, int startIndex)
		{
			if (startIndex >= actions.Length) {
				// out of memory
				return -1;
			}
			int endIndex = startIndex - 1;
			// expand the state into partitions
			SuccessorFunc.Partition(state, player, partition => {
				// each partition gets a thread local state and eval function
				var localState = stateCache.Get();
				var localEvalFunc = evalCache.Get();
				ManualResetEvent reset = new ManualResetEvent(false);
				resets.Add(reset);
				// queue each partition to be computed in parallel
				ThreadPool.QueueUserWorkItem(info => {
					// expand this partition into individual successor actions
					SuccessorFunc.Expand(localState, player, partition, action => {
						int i = Interlocked.Increment(ref endIndex);
						if (i < actions.Length) {
							// apply, evaluate, and store the successor action
							localState.ApplyAction(action);
							V eval = localEvalFunc.Evaluate(localState, player);
							Builder.InsertValue(action, eval);
							localState.UndoAction(action);
							actions[i] = action;
							return true;
						}
						// out of memory
						return false;
					});
					stateCache.Put(localState);
					evalCache.Put(localEvalFunc);
					reset.Set();
				});
			});
			// wait for all partitions to complete
			WaitHandle.WaitAll(resets.ToArray());
			resets.Clear();
			endIndex = Math.Min(actions.Length - 1, endIndex);
			int count = endIndex - startIndex + 1;
			if (count > 0) {
				Array.Sort(actions, startIndex, count, this);
				if (count > MaxExplorations) {
					// trim the lowest ranking actions if we expanded too many
					endIndex = startIndex + (int)MaxExplorations - 1;
				}
				return endIndex;
			}
			return -1;
		}
		
		public int Compare (A a0, A a1)
		{
			V v0 = Builder.ExtractValue(a0);
			V v1 = Builder.ExtractValue(a1);
			return Builder.Compare(v0, v1);
		}
		
		private void UpdateAllStates (A action)
		{
			state.ApplyAction(action);
			foreach  (var cached in stateCache.BackingStore) {
				cached.ApplyAction(action);
			}
		}
		
		private void UndoAllStates (A action)
		{
			state.UndoAction(action);
			foreach (var cached in stateCache.BackingStore) {
				cached.UndoAction(action);
			}
		}
		
		private A EndEarly (A action)
		{
			action = Builder.Clone(action);
			action = Builder.InsertValue(action, EvalFunc.Evaluate(state, maxPlayer));
			return action;
		}
	}
}
