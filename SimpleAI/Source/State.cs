namespace SimpleAI.Framework
{
	public interface State <A>
	{
		void ApplyAction (A action);
		
		void UndoAction (A action);
	}
}
