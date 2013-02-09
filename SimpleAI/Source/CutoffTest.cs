// http://paulmoore.mit-license.org

namespace SimpleAI.Framework
{
	/// <summary>
	/// A cutoff test is used during a search to determine if it should be ended early.
	/// This can be due to time or memory constraints, user input, etc.
	/// </summary>
	public interface CutoffTest <S, A> where S : State<A>
	{
		/// <summary>
		/// Invoked when the search is started.
		/// </summary>
		void Begin ();
		
		/// <summary>
		/// Invoked after the search has finished.
		/// </summary>
		void End ();
		
		/// <summary>
		/// Invoked to test if the search should be ended.
		/// </summary>
		/// <param name='state'>
		/// If <c>true</c>, the search will terminate as soon as possible.
		/// </param>
		bool Test (S state);
	}
}
