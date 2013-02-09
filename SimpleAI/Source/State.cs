// http://paulmoore.mit-license.org

namespace SimpleAI.Framework
{
	/// <summary>
	/// Represents a discrete state of a game.
	/// </summary>
	public interface State <A>
	{
		/// <summary>
		/// Apply an action to this state, making it a successor state.
		/// </summary>
		/// <param name='action'>
		/// The action to apply.
		/// </param>
		void ApplyAction (A action);
		
		/// <summary>
		/// Undos the action, returning it to the original state.
		/// </summary>
		/// <param name='action'>
		/// The action to unapply.
		/// </param>
		void UndoAction (A action);
	}
}
