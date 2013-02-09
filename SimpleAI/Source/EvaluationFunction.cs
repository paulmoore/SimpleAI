namespace SimpleAI.Framework
{
	/// <summary>
	/// An evaluation function takes a state, and evaluates how good it is for a player.
	/// This class does not need to be thread safe.
	/// </summary>
	public interface EvaluationFunction <S, A, V, P> : MutableClone<EvaluationFunction<S, A, V, P>> where S : State<A>
	{
		/// <summary>
		/// Evaluates how good this state is for a player and returns the result.
		/// </summary>
		/// <param name='state'>
		/// The state to evaluate.
		/// </param>
		/// <param name='player'>
		/// The player to evaluate the state for.
		/// </param>
		V Evaluate (S state, P player);
	}
}
