using System;

namespace SimpleAI.Framework
{
	public interface SuccessorFunction <S, A, F, P> where S : State<A>
	{
		void Partition (S state, P player, Action<F> partitioner);
		
		void Expand (S state, P player, F partition, Func<A, bool> expander);
	}
}
