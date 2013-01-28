namespace SimpleAI.Framework
{
	public interface EvaluationFunction <S, A, V, P> : MutableClone<EvaluationFunction<S, A, V, P>> where S : State<A>
	{
		V Evaluate (S state, P player);
	}
}
