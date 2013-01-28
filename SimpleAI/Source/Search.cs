using System;
using System.Threading;
using System.Collections.Generic;

namespace SimpleAI.Framework
{
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
		private int actionIndex;
		
		public uint InitialDepth
		{
			get;
			set;
		}
		
		public uint MaxExplorations
		{
			get;
			set;
		}
		
		public EvaluationFunction<S, A, V, P> EvalFunc
		{
			get;
			set;
		}
		
		public SuccessorFunction<S, A, F, P> SuccessorFunc
		{
			get;
			set;
		}
		
		public CutoffTest<S, A> Cutoff
		{
			get;
			set;
		}
		
		public ActionBuilder<A, V> Builder
		{
			get;
			set;
		}
		
		public Search (int maxActions)
		{
			int workers, ports;
			ThreadPool.GetAvailableThreads(out workers, out ports);
			resets = new List<ManualResetEvent>(workers);
			stateCache = new CloneCache<S>(workers);
			evalCache = new CloneCache<EvaluationFunction<S, A, V, P>>(workers);
			actions = new A[maxActions];
			InitialDepth = 2;
			MaxExplorations = uint.MaxValue;
		}
		
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
				actionIndex = -1;
				maxDepth = 0;
				A decision = MaxValue(default(A), Builder.NegInf, Builder.PosInf, 1);
				if (maxDepth > bestDepth && !isCutoff) {
					bestDepth = maxDepth;
					best = decision;
				}
			} while (!isCutoff);
			Cutoff.End();
			InitialDepth = bestDepth - 1;
			for (int i = 0; i < actions.Length; i++) {
				actions[i] = default(A);
			}
			this.maxPlayer = default(P);
			this.minPlayer = default(P);
			this.state = default(S);
			stateCache.Clear();
			stateCache.Parent = null;
			evalCache.Clear();
			evalCache.Parent = null;
			return best;
		}
		
		private A MaxValue (A parent, V alpha, V beta, uint depth)
		{
			maxDepth = Math.Max(maxDepth, depth);
			if (Cutoff.Test(state)) {
				isCutoff = true;
				return EndEarly(parent);
			}
			if (depth >= cutoffDepth) {
				return EndEarly(parent);
			}
			int startIndex = GenerateActions(maxPlayer);
			if (startIndex > actionIndex) {
				return EndEarly(parent);
			}
			int i = startIndex;
			V v = Builder.NegInf;
			A best = default(A);
			while (i <= actionIndex && !isCutoff) {
				A action = actions[i];
				UpdateAllStates(action);
				A minAction = MinValue(action, alpha, beta, depth + 1);
				V minValue = Builder.ExtractValue(minAction);
				UndoAllStates(action);
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
		
		private A MinValue (A parent, V alpha, V beta, uint depth)
		{
			maxDepth = Math.Max(maxDepth, depth);
			if (Cutoff.Test(state)) {
				isCutoff = true;
				return EndEarly(parent);
			}
			if (depth >= cutoffDepth) {
				return EndEarly(parent);
			}
			int startIndex = GenerateActions(minPlayer);
			if (startIndex > actionIndex) {
				return EndEarly(parent);
			}
			int i = startIndex;
			V v = Builder.PosInf;
			A best = default(A);
			while (i <= actionIndex && !isCutoff) {
				A action = actions[i];
				UpdateAllStates(action);
				A maxAction = MaxValue(action, alpha, beta, depth + 1);
				V maxValue = Builder.ExtractValue(maxAction);
				UndoAllStates(action);
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
		
		private int GenerateActions (P player)
		{
			int initialIndex = actionIndex;
			SuccessorFunc.Partition(state, player, delegate (F partition) {
				var localState = stateCache.Get();
				var localEvalFunc = evalCache.Get();
				ManualResetEvent reset = new ManualResetEvent(false);
				resets.Add(reset);
				ThreadPool.QueueUserWorkItem(delegate {
					SuccessorFunc.Expand(localState, player, partition, delegate (A action) {
						int i = Interlocked.Increment(ref actionIndex);
						if (i < actions.Length) {
							localState.ApplyAction(action);
							V eval = localEvalFunc.Evaluate(localState, player);
							Builder.InsertValue(action, eval);
							localState.UndoAction(action);
							actions[i] = action;
							return true;
						}
						Interlocked.Decrement(ref actionIndex);
						return false;
					});
					lock (stateCache) {
						stateCache.Put(localState);
						evalCache.Put(localEvalFunc);
					}
					reset.Set();
				});
			});
			WaitHandle.WaitAll(resets.ToArray());
			resets.Clear();
			actionIndex = Math.Min(actions.Length, actionIndex);
			int startIndex = initialIndex + 1;
			int count = actionIndex - initialIndex;
			if (count > 0) {
				Array.Sort(actions, startIndex, count, this);
				if (count > MaxExplorations) {
					actionIndex = initialIndex + (int)MaxExplorations;
				}
			}
			return startIndex;
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
			var list = stateCache.BackingStore;
			int count = list.Count;
			for (int i = 0; i < count; i++) {
				list[i].ApplyAction(action);
			}
		}
		
		private void UndoAllStates (A action)
		{
			state.UndoAction(action);
			var list = stateCache.BackingStore;
			int count = list.Count;
			for (int i = 0; i < count; i++) {
				list[i].UndoAction(action);
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
